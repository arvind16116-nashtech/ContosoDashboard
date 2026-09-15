# Research: Document Upload and Management

## Decision: Local file storage outside web root

The app is a local training application and therefore should not rely on cloud storage or external services. Files will be stored under `AppData/uploads` (or another secure local directory) rather than `wwwroot` to prevent direct browser access and to satisfy the security requirement. This keeps the solution predictable for offline use and makes the future Azure migration straightforward via `IFileStorageService`.

## Decision: Interface-based storage abstraction

A small storage abstraction is necessary because the training design explicitly calls for future cloud migration without rewriting business logic. The interface will include:

- `UploadAsync` for storing a file and returning a persisted metadata path
- `DeleteAsync` for removing the file from the backing store
- `DownloadAsync` for reading file bytes or a stream
- `GetUrlAsync` for generating a secure local or future cloud URL

This makes `DocumentService` independent of whether the underlying provider is local disk or Azure Blob Storage.

## Decision: Guid-based filenames

To avoid path traversal and duplicate-key issues, the app will generate a GUID-based filename before writing to disk. The final path pattern is:

`{userId}/{projectId or "personal"}/{guid}.{extension}`

This ensures unique storage values while preserving project and user organization.

## Decision: Database model simplification

The existing app uses integer IDs for users and projects. The document feature will match that pattern and store `DocumentId` as an integer. Category values are stored as strings as requested, rather than enum-backed integer values, to avoid unnecessary conversion code and keep the metadata simple.

## Decision: Authorization layering

Access checks must be performed in the service layer so pages and components cannot bypass them. This keeps the app secure and reduces the risk of insecure direct object reference (IDOR) flaws.

## Decision: Notification and integration approach

The dashboard, task pages, and shared-with-me views should consume the same document service methods and notification service. That reduces duplication and ensures all file events behave consistently across the app.

## Decision: Audit logging

Document actions are recorded in an audit-friendly service pattern, even if the initial implementation stores only service logs or notification records. The data model will support future reporting and compliance queries without changing the app’s overall architecture.
