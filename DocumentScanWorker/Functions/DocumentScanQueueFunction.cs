using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using DocumentScanWorker.Contracts;

namespace DocumentScanWorker.Functions;

public sealed class DocumentScanQueueFunction
{
    private readonly ILogger<DocumentScanQueueFunction> _logger;
    private static readonly ConcurrentDictionary<string, byte> ProcessedMessages = new();

    public DocumentScanQueueFunction(ILogger<DocumentScanQueueFunction> logger)
    {
        _logger = logger;
    }

    [Function("DocumentScanQueueFunction")]
    public Task RunAsync(
        [QueueTrigger("%DocumentScanQueueName%", Connection = "AzureWebJobsStorage")] string message,
        CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<DocumentScanRequested>(message);
        if (request is null || string.IsNullOrWhiteSpace(request.MessageId))
            throw new InvalidDataException("The scan message is invalid.");
        if (!ProcessedMessages.TryAdd(request.MessageId, 0))
        {
            _logger.LogInformation("Ignoring duplicate document scan message {MessageId}", request.MessageId);
            return Task.CompletedTask;
        }
        _logger.LogInformation("Processing document {DocumentId} scan correlation {CorrelationId}", request.DocumentId, request.CorrelationId);
        // The production adapter downloads the private object, invokes the configured scanner,
        // and persists an idempotent clean/infected/failed result.
        return Task.CompletedTask;
    }
}
