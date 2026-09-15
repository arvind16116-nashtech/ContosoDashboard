# Document Scan Worker

Production-only Azure Functions worker for asynchronous document scanning.

- Queue trigger: `DocumentScanRequested`
- Queue messages contain metadata and opaque storage keys, never file bytes or physical paths.
- Clean files become available; infected files are quarantined; transient errors retry and poison messages remain unavailable.
- The offline ContosoDashboard training runtime uses `LocalDocumentScanService` and does not require Azure Functions or Queue Storage.
