# Quickstart Validation: Document Upload and Management

## Offline Training Validation

```powershell
dotnet restore .\ContosoDashboard\ContosoDashboard.csproj
dotnet build .\ContosoDashboard\ContosoDashboard.csproj --no-restore
dotnet run --project .\ContosoDashboard\ContosoDashboard.csproj
```

Sign in with a seeded mock user and validate supported/unsupported files, 25 MB rejection, per-file metadata, authorization, sharing, notification preferences, replacement failure, deletion cleanup, and audit records. The local path uses deterministic type, size, and extension validation and requires no Azure services.

## Production-Like Queue Validation

Configure an isolated Azure Storage account or approved Queue Storage emulator and a private blob/storage implementation. Deploy the Azure Function with its Queue Storage trigger and scanner adapter in a non-production environment.

1. Upload a supported file through the web application.
2. Confirm metadata is `Pending` and preview/download is denied before processing.
3. Confirm one versioned `DocumentScanRequested` message is placed on the scan queue without file bytes or physical paths.
4. Process a clean fixture and confirm the Function changes the document to `Clean` and available.
5. Process an infected fixture and confirm the document is rejected/quarantined and remains inaccessible.
6. Force a transient scanner failure and confirm queue retry; exceed the retry threshold and confirm poison-message handling leaves the document unavailable.
7. Redeliver the same message and confirm idempotent state and audit behavior.
8. Force queue publication failure after metadata commit and confirm reconciliation republishes the request without making the document available.

Expected result: no document is accessible before a clean result, failures are recoverable, and duplicate deliveries do not create inconsistent state.

Performance targets remain: upload up to 25 MB within 30 seconds, list/search within 2 seconds for 500 accessible documents, and preview within 3 seconds after a clean result.
