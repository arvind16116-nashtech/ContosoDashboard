# Document Upload Contracts

## Upload document

### Request

- `Title` (string, required)
- `Description` (string, optional)
- `Category` (string, required; one of Project Documents, Team Resources, Personal Files, Reports, Presentations, Other)
- `ProjectId` (int, optional)
- `Tags` (string, optional)
- `File` (binary upload, required)

### Validation

- File size must be <= 25 MB.
- File extension must be in the allowed set for PDF, office documents, text files, or common images.
- File must be stored only after unique path generation and successful save to the backing file store.

### Response

- `DocumentId` (int)
- `Title` (string)
- `Category` (string)
- `StoredFilePath` (string)
- `UploadedAtUtc` (DateTime)
- `Message` (string)

## Download document

### Authorization

- User must have access rights via owner, project membership, or document share.

### Response

- Binary file stream or secure download payload
- `Content-Type` set based on MIME type

## Share document

### Request

- `DocumentId` (int)
- `UserId` (int)
- `Permission` (string)

### Response

- Share confirmation
- Recipient notification record

## Error contract

- Validation errors return clear user-friendly messages.
- File-not-found and unauthorized actions return a non-sensitive, application-level error response.
