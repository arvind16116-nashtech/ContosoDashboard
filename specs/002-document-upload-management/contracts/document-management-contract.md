# Document Management Contract

## Upload and Scan Contract

1. Validate each selected file and its independent metadata.
2. Save the file to private storage and persist metadata as unavailable with `ScanStatus = Pending`.
3. In production, publish a versioned `DocumentScanRequested` message to Azure Queue Storage after metadata commit.
4. An Azure Function Queue Storage trigger processes the message asynchronously.
5. Only a clean scan result makes the document previewable/downloadable. Infected files are rejected or quarantined; transient failures remain unavailable and are retried.
6. Duplicate queue deliveries must not duplicate terminal state transitions or audit records.

Queue message fields: `MessageId`, `SchemaVersion`, `DocumentId`, `StorageObjectKey`, `ContentType`, `FileSize`, and `CorrelationId`. Messages must not contain file bytes, secrets, or physical filesystem roots.

## Service Contract

The web application provides operations equivalent to:

- `UploadAsync(currentUser, file, metadata)`
- `GetAccessibleAsync(currentUser, query)`
- `GetSharedWithMeAsync(currentUser, query)`
- `GetByProjectAsync(currentUser, projectId)`
- `GetByTaskAsync(currentUser, taskId)`
- `UpdateMetadataAsync(currentUser, documentId, metadata)`
- `ReplaceAsync(currentUser, documentId, file, metadata)`
- `DeleteAsync(currentUser, documentId)`
- `ShareAsync(currentUser, documentId, recipients)`
- `GetActivityReportAsync(currentUser, query)`

The production worker provides an operation equivalent to `ProcessScanRequestAsync(message)`, which must be idempotent and must not bypass document authorization.

## Storage and Worker Contract

`IFileStorageService` uploads, downloads, deletes, and quarantines by opaque relative/object keys. Implementations reject rooted paths, traversal segments, and locations outside the configured root.

`IDocumentScanService` or equivalent scan boundary supports the local deterministic training result and the production worker’s malware scanner adapter. Azure Functions and Queue Storage are production deployment components; local training must not require them.

## Access and Error Behavior

- Preview and download deny pending, failed, infected, quarantined, deleted, or unauthorized documents without disclosing whether an identifier exists.
- Queue publication failure leaves the document unavailable and retryable; it never grants access.
- Scanner failure uses queue retries and poison-message handling; terminal operational failures remain unavailable and are visible to administrators.
- Notification failure does not roll back a successful document state transition and is logged safely.
