using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentScanQueue
{
    Task QueueAsync(Document document, CancellationToken cancellationToken = default);
}

public sealed class DocumentScanQueue : IDocumentScanQueue
{
    private readonly ILogger<DocumentScanQueue> _logger;

    public DocumentScanQueue(ILogger<DocumentScanQueue> logger)
    {
        _logger = logger;
    }

    public Task QueueAsync(Document document, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Local scan queued for document {DocumentId} with correlation {CorrelationId}", document.DocumentId, document.ScanCorrelationId);
        return Task.CompletedTask;
    }
}
