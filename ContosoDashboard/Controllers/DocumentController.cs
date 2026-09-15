using System.Security.Claims;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoDashboard.Controllers;

[ApiController]
[Authorize(Policy = "Employee")]
[Route("api/documents")]
public sealed class DocumentController : ControllerBase
{
    private const long MaxFileSize = 25 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string[]> SupportedFileTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ["application/pdf"],
            [".doc"] = ["application/msword"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            [".xls"] = ["application/vnd.ms-excel"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            [".ppt"] = ["application/vnd.ms-powerpoint"],
            [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
            [".txt"] = ["text/plain"],
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"]
        };

    private readonly IDocumentService _documentService;

    public DocumentController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize + 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] DocumentUploadForm form,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new { error = "The authenticated user could not be identified." });
        }

        if (form.File is null || form.File.Length == 0)
        {
            return BadRequest(new { error = "A non-empty file is required." });
        }

        if (form.File.Length > MaxFileSize)
        {
            return BadRequest(new { error = "The file cannot exceed 25 MB." });
        }

        var extension = Path.GetExtension(form.File.FileName);
        if (!SupportedFileTypes.TryGetValue(extension, out var contentTypes) ||
            !contentTypes.Contains(form.File.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "The selected file type is not supported." });
        }

        if (string.IsNullOrWhiteSpace(form.Title))
        {
            return BadRequest(new { error = "A document title is required." });
        }

        if (string.IsNullOrWhiteSpace(form.Category))
        {
            return BadRequest(new { error = "A document category is required." });
        }

        var result = await _documentService.UploadAsync(
            userId,
            form.File,
            new DocumentUploadMetadata(
                form.Title.Trim(),
                form.Description,
                form.Category.Trim(),
                form.ProjectId,
                form.TaskId,
                form.Tags),
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Error ?? "The document could not be uploaded." });
        }

        return Created($"/api/documents/{result.DocumentId}", new
        {
            documentId = result.DocumentId,
            message = "The document was uploaded successfully."
        });
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] int? projectId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return Ok(await _documentService.GetAccessibleAsync(userId, search, projectId, cancellationToken));
    }

    [HttpGet("{documentId:int}/download")]
    public async Task<IActionResult> Download(int documentId, CancellationToken cancellationToken)
        => await StreamDocument(documentId, inline: false, cancellationToken);

    [HttpGet("{documentId:int}/preview")]
    public async Task<IActionResult> Preview(int documentId, CancellationToken cancellationToken)
        => await StreamDocument(documentId, inline: true, cancellationToken);

    private async Task<IActionResult> StreamDocument(int documentId, bool inline, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var document = await _documentService.GetAuthorizedAsync(userId, documentId, cancellationToken);
        if (document is null) return NotFound();
        var stream = await HttpContext.RequestServices.GetRequiredService<IFileStorageService>()
            .DownloadAsync(document.StoredFilePath, cancellationToken);
        if (stream is null) return NotFound();
        if (inline && document.FileType is not ("application/pdf" or "image/jpeg" or "image/png"))
        {
            await stream.DisposeAsync();
            return BadRequest(new { error = "Preview is not supported for this file type." });
        }
        return File(stream, document.FileType, inline ? null : document.OriginalFileName, enableRangeProcessing: true);
    }

    [HttpDelete("{documentId:int}")]
    public async Task<IActionResult> Delete(int documentId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return await _documentService.DeleteAsync(userId, documentId, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpPost("{documentId:int}/share/{recipientUserId:int}")]
    public async Task<IActionResult> Share(int documentId, int recipientUserId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        return await _documentService.ShareAsync(userId, documentId, recipientUserId, cancellationToken)
            ? Ok(new { message = "Document shared." })
            : BadRequest(new { error = "The document could not be shared with that recipient." });
    }

    private bool TryGetUserId(out int userId)
        => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}

public sealed class DocumentUploadForm
{
    public IFormFile? File { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string? Tags { get; set; }
}
