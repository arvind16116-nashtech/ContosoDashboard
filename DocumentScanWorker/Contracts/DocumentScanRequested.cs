namespace DocumentScanWorker.Contracts;

public sealed record DocumentScanRequested(
    string MessageId,
    string SchemaVersion,
    int DocumentId,
    string StorageObjectKey,
    string ContentType,
    long FileSize,
    string CorrelationId);
