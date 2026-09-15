using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public sealed class DocumentAuthorizationService
{
    private readonly ApplicationDbContext _context;

    public DocumentAuthorizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Document> AccessibleDocuments(int userId)
    {
        return _context.Documents.Where(d => d.IsAvailable && d.ScanStatus == DocumentScanStatuses.Clean &&
            (d.UploadedByUserId == userId ||
             d.Project!.ProjectManagerId == userId ||
             d.Project!.ProjectMembers.Any(pm => pm.UserId == userId) ||
             d.Shares.Any(s => s.IsActive && s.RecipientUserId == userId)));
    }

    public async Task<bool> CanUploadToProjectAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects.AnyAsync(p => p.ProjectId == projectId &&
            (p.ProjectManagerId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId)), cancellationToken);
    }

    public async Task<bool> CanAccessAsync(Document document, int userId, CancellationToken cancellationToken = default)
    {
        if (!document.IsAvailable || document.ScanStatus != DocumentScanStatuses.Clean) return false;
        if (document.UploadedByUserId == userId) return true;
        if (document.Project?.ProjectManagerId == userId) return true;
        if (document.Project?.ProjectMembers.Any(pm => pm.UserId == userId) == true) return true;
        return document.Shares.Any(s => s.IsActive && s.RecipientUserId == userId);
    }
}
