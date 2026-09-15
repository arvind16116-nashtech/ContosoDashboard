# Data Model: Document Upload and Management

## Document

Represents an uploaded work file and its searchable metadata.

| Field | Type | Rules |
|---|---|---|
| DocumentId | integer | Required primary key |
| Title | text | Required and non-empty |
| Description | text | Optional |
| Category | text | Required predefined text value |
| Tags | text | Optional normalized search text |
| OriginalFileName | text | Required display name; never a storage path |
| StoredFilePath | text | Required opaque relative path outside `wwwroot` |
| FileType | text | Required MIME type, up to 255 characters |
| FileSize | integer | Greater than 0 and no more than 25 MB |
| UploadedDate | datetime | Required |
| UploadedByUserId | integer | Required User foreign key |
| ProjectId | integer | Optional Project foreign key |
| TaskId | integer | Optional TaskItem foreign key; must match ProjectId |
| ScanStatus | text | Pending, Clean, Infected, Failed, or Quarantined |
| ScanAttemptCount | integer | Non-negative retry count |
| ScanCorrelationId | text | Required for production async processing; unique per file version |
| IsAvailable | boolean | False until Clean; false after deletion/quarantine |

## DocumentScanRequest

Represents the durable handoff to asynchronous scanning. It may be implemented as an outbox record, queue message contract, or both.

| Field | Type | Rules |
|---|---|---|
| MessageId | text | Required idempotency key |
| DocumentId | integer | Required |
| StorageObjectKey | text | Opaque relative/blob key; no physical root |
| ContentType | text | Required |
| FileSize | integer | Required |
| SchemaVersion | text | Required versioned contract |
| CorrelationId | text | Required tracing identifier |
| CreatedDate | datetime | Required |
| PublishedDate | datetime | Optional until queued |

## DocumentShare and DocumentActivity

`DocumentShare` tracks active user/team sharing relationships. `DocumentActivity` records upload, scan outcome, preview, download, update, replacement, deletion, and share actions with actor, document, timestamp, and safe details.

## Relationships and State Transitions

- User, Project, and TaskItem relate to Document through integer foreign keys.
- Document has many shares, activities, and scan attempts/requests.
- Upload: Pending validation -> Pending scan -> Clean/available.
- Scan infection: Pending scan -> Infected or Quarantined/unavailable.
- Retryable scanner failure: Pending scan -> Failed/unavailable -> Pending scan on retry.
- Replacement creates a new file version/correlation state and preserves the prior available file until the replacement is Clean.
- Delete transitions the document to unavailable and removes or quarantines the stored file according to policy.

## Invariants

- Preview/download authorization checks `IsAvailable`, `ScanStatus = Clean`, current role, ownership, project membership, or active share.
- A task association requires the same project association.
- Queue processing is idempotent by `MessageId` and document file-version correlation ID.
- Infected or quarantined documents can never be returned by search/list queries.
- Notification preferences affect delivery only and never access.
