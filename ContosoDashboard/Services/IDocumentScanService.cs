using Microsoft.AspNetCore.Http;

namespace ContosoDashboard.Services;

public interface IDocumentScanService
{
    Task<DocumentScanValidationResult> ValidateAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public sealed record DocumentScanValidationResult(bool IsValid, string? Error = null);
