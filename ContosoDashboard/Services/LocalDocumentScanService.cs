using Microsoft.AspNetCore.Http;

namespace ContosoDashboard.Services;

public sealed class LocalDocumentScanService : IDocumentScanService
{
    private const long MaxFileSize = 25 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string[]> SupportedTypes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"], [".doc"] = ["application/msword"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        [".xls"] = ["application/vnd.ms-excel"],
        [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
        [".ppt"] = ["application/vnd.ms-powerpoint"],
        [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
        [".txt"] = ["text/plain"], [".jpg"] = ["image/jpeg"], [".jpeg"] = ["image/jpeg"], [".png"] = ["image/png"]
    };

    public Task<DocumentScanValidationResult> ValidateAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0) return Task.FromResult(new DocumentScanValidationResult(false, "The file is empty."));
        if (file.Length > MaxFileSize) return Task.FromResult(new DocumentScanValidationResult(false, "The file cannot exceed 25 MB."));
        var extension = Path.GetExtension(file.FileName);
        if (!SupportedTypes.TryGetValue(extension, out var contentTypes) || !contentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return Task.FromResult(new DocumentScanValidationResult(false, "The selected file type is not supported."));
        return Task.FromResult(new DocumentScanValidationResult(true));
    }
}
