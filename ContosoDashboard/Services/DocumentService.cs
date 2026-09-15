using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public sealed class DocumentService : IDocumentService
{
    private static readonly string[] Categories =
    ["Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other"];

    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly IDocumentScanService _scanService;
    private readonly DocumentAuthorizationService _authorization;
    private readonly IDocumentScanQueue _scanQueue;
    private readonly INotificationService _notificationService;

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService storage,
        IDocumentScanService scanService,
        DocumentAuthorizationService authorization,
        IDocumentScanQueue scanQueue,
        INotificationService notificationService)
    {
        _context = context;
        _storage = storage;
        _scanService = scanService;
        _authorization = authorization;
        _scanQueue = scanQueue;
        _notificationService = notificationService;
    }

    public async Task<DocumentUploadResult> UploadAsync(int userId, IFormFile file, DocumentUploadMetadata metadata, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(metadata.Title)) return Failure("A document title is required.");
        if (!Categories.Contains(metadata.Category, StringComparer.OrdinalIgnoreCase)) return Failure("The document category is invalid.");

        var validation = await _scanService.ValidateAsync(file, cancellationToken);
        if (!validation.IsValid) return Failure(validation.Error ?? "The file failed validation.");

        if (metadata.ProjectId.HasValue && !await _authorization.CanUploadToProjectAsync(userId, metadata.ProjectId.Value, cancellationToken))
            return Failure("You are not authorized to upload to this project.");

        if (metadata.TaskId.HasValue)
        {
            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.TaskId == metadata.TaskId.Value, cancellationToken);
            if (task is null || task.ProjectId != metadata.ProjectId) return Failure("The task must belong to the selected project.");
        }

        string? storedPath = null;
        try
        {
            await using var input = file.OpenReadStream();
            storedPath = await _storage.UploadAsync(input, file.FileName, file.ContentType, userId, metadata.ProjectId, cancellationToken);

            var document = new Document
            {
                Title = metadata.Title.Trim(),
                Description = metadata.Description?.Trim(),
                Category = metadata.Category.Trim(),
                Tags = metadata.Tags?.Trim(),
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFilePath = storedPath,
                FileType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId,
                ProjectId = metadata.ProjectId,
                TaskId = metadata.TaskId,
                ScanStatus = DocumentScanStatuses.Clean,
                IsAvailable = true
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);
            _context.DocumentActivities.Add(new DocumentActivity
            {
                DocumentId = document.DocumentId,
                ActorUserId = userId,
                ActivityType = "Upload",
                Details = "Document uploaded and passed local validation."
            });
            await _context.SaveChangesAsync(cancellationToken);
            await _scanQueue.QueueAsync(document, cancellationToken);
            return new DocumentUploadResult(true, document.DocumentId);
        }
        catch (Exception ex) when (ex is IOException or DbUpdateException)
        {
            if (storedPath is not null)
            {
                try { await _storage.DeleteAsync(storedPath, cancellationToken); } catch { }
            }
            return Failure("The document could not be stored. Please try again.");
        }
    }

    public async Task<List<DocumentListItem>> GetAccessibleAsync(int userId, string? search = null, int? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = _authorization.AccessibleDocuments(userId).Include(d => d.Project).AsNoTracking();
        if (projectId.HasValue) query = query.Where(d => d.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Title.Contains(term) || (d.Description != null && d.Description.Contains(term)) ||
                (d.Tags != null && d.Tags.Contains(term)) || d.OriginalFileName.Contains(term));
        }
        return await query.OrderByDescending(d => d.UploadedDate).Select(ToListItem).ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentListItem>> GetByProjectAsync(int userId, int projectId, CancellationToken cancellationToken = default)
        => await GetAccessibleAsync(userId, null, projectId, cancellationToken);

    public Task<Document?> GetAuthorizedAsync(int userId, int documentId, CancellationToken cancellationToken = default)
        => _authorization.AccessibleDocuments(userId).Include(d => d.Project).Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);

    public async Task<bool> UpdateMetadataAsync(int userId, int documentId, DocumentUploadMetadata metadata, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && d.UploadedByUserId == userId, cancellationToken);
        if (document is null || string.IsNullOrWhiteSpace(metadata.Title) || !Categories.Contains(metadata.Category, StringComparer.OrdinalIgnoreCase)) return false;
        document.Title = metadata.Title.Trim();
        document.Description = metadata.Description?.Trim();
        document.Category = metadata.Category.Trim();
        document.Tags = metadata.Tags?.Trim();
        _context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, ActorUserId = userId, ActivityType = "Update" });
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && d.UploadedByUserId == userId, cancellationToken);
        if (document is null) return false;
        document.IsAvailable = false;
        document.ScanStatus = DocumentScanStatuses.Quarantined;
        _context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, ActorUserId = userId, ActivityType = "Delete" });
        await _context.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(document.StoredFilePath, cancellationToken);
        return true;
    }

    public Task<List<DocumentActivity>> GetActivityAsync(int userId, CancellationToken cancellationToken = default)
        => _context.DocumentActivities.Include(a => a.Document).Where(a => a.ActorUserId == userId || _context.Users.Any(u => u.UserId == userId && u.Role == UserRole.Administrator))
            .OrderByDescending(a => a.OccurredDate).Take(200).ToListAsync(cancellationToken);

    public async Task<bool> ReplaceAsync(int userId, int documentId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId && d.UploadedByUserId == userId && d.IsAvailable, cancellationToken);
        var validation = await _scanService.ValidateAsync(file, cancellationToken);
        if (document is null || !validation.IsValid) return false;
        string? replacementPath = null;
        try
        {
            await using var input = file.OpenReadStream();
            replacementPath = await _storage.UploadAsync(input, file.FileName, file.ContentType, userId, document.ProjectId, cancellationToken);
            var oldPath = document.StoredFilePath;
            document.StoredFilePath = replacementPath;
            document.OriginalFileName = Path.GetFileName(file.FileName);
            document.FileType = file.ContentType;
            document.FileSize = file.Length;
            document.ScanCorrelationId = Guid.NewGuid().ToString("N");
            _context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, ActorUserId = userId, ActivityType = "Replace" });
            await _context.SaveChangesAsync(cancellationToken);
            await _storage.DeleteAsync(oldPath, cancellationToken);
            return true;
        }
        catch
        {
            if (replacementPath is not null) { try { await _storage.DeleteAsync(replacementPath, cancellationToken); } catch { } }
            return false;
        }
    }

    public async Task<bool> ShareAsync(int userId, int documentId, int recipientUserId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.Include(d => d.Project).FirstOrDefaultAsync(d => d.DocumentId == documentId && d.UploadedByUserId == userId, cancellationToken);
        var recipient = await _context.Users.FindAsync([recipientUserId], cancellationToken);
        if (document is null || recipient is null || (document.ProjectId.HasValue && !await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == document.ProjectId && pm.UserId == recipientUserId, cancellationToken))) return false;
        _context.DocumentShares.Add(new DocumentShare { DocumentId = documentId, RecipientUserId = recipientUserId, SharedByUserId = userId });
        _context.DocumentActivities.Add(new DocumentActivity { DocumentId = documentId, ActorUserId = userId, ActivityType = "Share" });
        await _context.SaveChangesAsync(cancellationToken);
        if (recipient.InAppNotificationsEnabled)
        {
            await _notificationService.CreateNotificationAsync(new Notification { UserId = recipientUserId, Title = "Document Shared", Message = $"A document was shared with you: {document.Title}", Type = NotificationType.ProjectUpdate, Priority = NotificationPriority.Informational });
        }
        return true;
    }

    private static DocumentUploadResult Failure(string error) => new(false, Error: error);

    private static readonly System.Linq.Expressions.Expression<Func<Document, DocumentListItem>> ToListItem =
        d => new DocumentListItem(d.DocumentId, d.Title, d.Category, d.OriginalFileName, d.FileType, d.FileSize, d.UploadedDate, d.Project == null ? null : d.Project.Name, d.ProjectId, d.ScanStatus);
}