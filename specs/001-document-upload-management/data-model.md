# Data Model: Document Upload and Management

## Core Entities

### Document

Represents a stored file and its metadata.

- `DocumentId` (int, PK)
- `Title` (string, required, max 255)
- `Description` (string, nullable, max 2000)
- `Category` (string, required)
- `FileName` (string, required)
- `StoredFilePath` (string, required)
- `FileSizeBytes` (long)
- `MimeType` (string, max 255)
- `UploadedByUserId` (int, FK to `User`)
- `ProjectId` (int?, FK to `Project`)
- `TaskId` (int?, optional future relation for task attachments)
- `UploadedAtUtc` (DateTime)
- `UpdatedAtUtc` (DateTime)
- `IsDeleted` (bool)
- `Tags` (string, nullable, search-friendly text)

### DocumentShare

Represents a share relationship between a document and a user.

- `DocumentShareId` (int, PK)
- `DocumentId` (int, FK to `Document`)
- `UserId` (int, FK to `User`)
- `SharedByUserId` (int, FK to `User`)
- `SharedAtUtc` (DateTime)
- `Permission` (string, e.g., "View" or "Manage")

## Relationships

- A `User` can upload many `Document` records.
- A `Project` can own many `Document` records (optional for personal documents).
- A `Document` may have many `DocumentShare` records.
- A `User` can receive many share grants through `DocumentShare`.

## Indexes

- Index on `Document.ProjectId` for project document queries
- Index on `Document.UploadedByUserId` for personal document list pages
- Index on `Document.Category` for filtering and category-based views
- Index on `DocumentShare.UserId` for shared-with-me views
- Composite index on `Document.ProjectId + Document.Category` for filtered browsing

## Notes

The model should be added to `ApplicationDbContext` with `DbSet<Document>` and `DbSet<DocumentShare>`, then seeded minimal data only for testing or demonstration.
