using Microsoft.AspNetCore.Http;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<DocumentUploadResult> UploadAsync(
        int userId,
        IFormFile file,
        DocumentUploadMetadata metadata,
        CancellationToken cancellationToken = default);
    Task<List<DocumentListItem>> GetAccessibleAsync(int userId, string? search = null, int? projectId = null, CancellationToken cancellationToken = default);
    Task<List<DocumentListItem>> GetByProjectAsync(int userId, int projectId, CancellationToken cancellationToken = default);
    Task<Document?> GetAuthorizedAsync(int userId, int documentId, CancellationToken cancellationToken = default);
    Task<bool> UpdateMetadataAsync(int userId, int documentId, DocumentUploadMetadata metadata, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default);
    Task<List<DocumentActivity>> GetActivityAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ReplaceAsync(int userId, int documentId, IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> ShareAsync(int userId, int documentId, int recipientUserId, CancellationToken cancellationToken = default);
}

public sealed record DocumentUploadMetadata(
    string Title,
    string? Description,
    string Category,
    int? ProjectId,
    int? TaskId,
    string? Tags);

public sealed record DocumentUploadResult(
    bool Succeeded,
    int? DocumentId = null,
    string? Error = null);

public sealed record DocumentListItem(
    int DocumentId,
    string Title,
    string Category,
    string OriginalFileName,
    string FileType,
    long FileSize,
    DateTime UploadedDate,
    string? ProjectName,
    int? ProjectId,
    string ScanStatus);
