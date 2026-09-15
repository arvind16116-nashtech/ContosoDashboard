# Implementation Plan: Document Upload and Management

**Branch**: `002-document-upload-management` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `specs/002-document-upload-management/spec.md`

## Summary

Add offline document upload, organization, retrieval, sharing, project/task/dashboard integration, and administrator audit reporting to the existing Blazor Server application. Metadata will use EF Core SQLite, file bytes will use an authorized local filesystem storage service outside `wwwroot`, and all document operations will enforce current-user permissions. Production deployments will add asynchronous virus scanning through an Azure Function triggered by Azure Queue Storage after upload; the offline training path will use deterministic type, size, and extension validation without requiring Azure services.

## Technical Context

**Language/Version**: C# on .NET 10 for the web application; Azure Functions isolated worker for the production scanning worker  
**Primary Dependencies**: ASP.NET Core Blazor Server, Entity Framework Core 10 SQLite, existing cookie mock authentication, Azure Functions, Azure Queue Storage SDK  
**Storage**: SQLite metadata database plus local filesystem files outside `wwwroot` for training; production can use Azure Blob Storage and Queue Storage through the storage abstraction  
**Testing**: Focused .NET tests for validation, authorization, storage cleanup, queue message creation, scan-result handling, and workflow failures; `dotnet restore`, `dotnet build`, runtime smoke validation, and an Azure integration test using an emulator or isolated test storage  
**Target Platform**: Windows ARM64-compatible offline training environment; Azure-hosted production worker  
**Project Type**: Single web application with a separately deployable production background worker  
**Performance Goals**: Upload up to 25 MB within 30 seconds; list and search within 2 seconds for up to 500 accessible documents; preview within 3 seconds; queue scan work immediately after upload and process it asynchronously without blocking the upload request  
**Constraints**: Offline training remains cloud-free; SQLite; integer document keys; text categories; files outside `wwwroot`; direct access blocked until scan status is safe; mock authentication remains training-only; no Azure SDK dependency is required for local training execution  
**Scale/Scope**: Five prioritized user journeys, 27 functional requirements, up to 500 documents in list-performance validation, existing seeded users/projects/tasks, and one asynchronous scan workflow for each accepted upload

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Secure-by-Default Access**: PASS. Authorization remains in document services and preview/download boundaries. Documents remain unavailable until scan status is safe, and queue/worker messages contain no file contents.
- **Offline-First Architecture**: PASS. Local training uses SQLite and local filesystem storage with no Azure dependency. Azure Functions and Queue Storage are an explicitly isolated production implementation.
- **Evidence-Driven Delivery**: PASS. Tests cover queue publication and scan state transitions; restore, build, runtime, and queue-worker validation are documented.
- **Minimal-Complexity Design**: PASS. The background worker is introduced only for the explicit asynchronous virus-scanning requirement and is isolated behind a small scan contract. No worker is required for local training.
- **Traceable Feature Work**: PASS. The design is tied to the feature specification, this plan, research, data model, contracts, quickstart, and future task list.
- **Gate status before research**: PASS; no unresolved technical clarifications.
- **Gate status after design**: PASS; the cloud worker is optional for training and does not weaken local security or access controls.

## Phase 0: Research

Research decisions are recorded in [research.md](research.md). The key choices are:

- Extend the existing Blazor Server layered architecture.
- Use SQLite and local filesystem storage for training.
- Use an `IFileStorageService` abstraction for local files and future blob storage.
- Publish a small scan-request message to Queue Storage after durable upload metadata is created.
- Process scans asynchronously in an Azure Function with a Queue Storage trigger in production.
- Keep documents unavailable to preview/download until the worker records a safe result; reject or quarantine unsafe files.
- Use deterministic type/size/extension validation in training because no cloud services are required offline.

## Phase 1: Design

- Entities, relationships, scan state, and validation rules: [data-model.md](data-model.md).
- User, service, storage, and scan-worker contracts: [contracts/document-management-contract.md](contracts/document-management-contract.md).
- Runnable local and production-like validation scenarios: [quickstart.md](quickstart.md).

## Asynchronous Virus-Scanning Workflow

1. The web application validates each selected file’s extension, MIME type, size, metadata, and authorization.
2. The storage abstraction writes the file to a private pending location and returns an opaque relative path.
3. The application persists the document metadata with `ScanStatus = Pending` and `IsAvailable = false`.
4. After the metadata transaction commits, the application publishes a versioned `DocumentScanRequested` message to an Azure Queue Storage queue. The message contains `DocumentId`, storage object/path identifier, content type, size, and correlation ID; it does not contain file bytes or user-controlled physical paths.
5. An Azure Function using a Queue Storage trigger reads the message, downloads the private object through the storage abstraction, and invokes the configured malware scanner.
6. The Function records an idempotent scan result: `Clean` changes the document to available; `Infected` changes it to rejected/quarantined and prevents access; `Error` leaves it unavailable and schedules retry according to queue retry/poison-message handling.
7. The Function emits structured logs and correlation IDs. Repeated delivery of the same message must not duplicate audit entries or incorrectly reverse a terminal infected state.
8. The application notifies the uploader only according to document notification preferences and never treats notification delivery as the access decision.

### Offline Training Behavior

The local implementation performs deterministic type, size, and extension validation synchronously. It marks a validated file safe within the local workflow and does not require Azure Functions, Queue Storage, Blob Storage, or a malware engine. The production scan boundary remains represented by an interface and state model so the worker can be added without changing document authorization or UI contracts.

### Failure and Recovery Rules

- If file storage fails, no document metadata or scan message is committed.
- If metadata commit fails after file storage, the application removes the pending file or records cleanup work; no accessible record is created.
- If queue publication fails after metadata commit, the document remains unavailable and a retryable outbox/reconciliation path must republish the scan request; it must never become available by timeout.
- If a scan fails transiently, Queue Storage retries the message. After the poison-message threshold, the document remains unavailable and an administrator-visible operational error is recorded.
- If a scanner reports infection, the file is quarantined or deleted according to deployment policy and all preview/download requests remain denied.

## Project Structure

### Documentation (this feature)

```text
specs/002-document-upload-management/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── document-management-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md                 # generated by /speckit.tasks
```

### Source Code (repository root)

```text
ContosoDashboard/
├── Data/ApplicationDbContext.cs
├── Models/
│   ├── Document.cs
│   ├── DocumentShare.cs
│   └── DocumentActivity.cs
├── Services/
│   ├── IDocumentService.cs
│   ├── DocumentService.cs
│   ├── IFileStorageService.cs
│   ├── LocalFileStorageService.cs
│   ├── IDocumentScanService.cs
│   └── LocalDocumentScanService.cs
├── Pages/
│   ├── Documents.razor
│   ├── SharedDocuments.razor
│   ├── DocumentActivity.razor
│   ├── ProjectDetails.razor
│   ├── Tasks.razor
│   └── Index.razor
└── Controllers/DocumentController.cs

DocumentScanWorker/                              # production-only deployment project
├── Functions/DocumentScanQueueFunction.cs
├── Services/IMalwareScanner.cs
├── Services/AzureMalwareScanner.cs
└── Program.cs

tests/ContosoDashboard.Tests/
├── DocumentValidationTests.cs
├── DocumentAuthorizationTests.cs
├── LocalFileStorageServiceTests.cs
├── DocumentServiceWorkflowTests.cs
└── DocumentScanQueueTests.cs
```

**Structure Decision**: Keep the user-facing feature in the existing web project. Add a small separately deployable Azure Functions worker for production scan processing, sharing contracts and storage abstractions rather than web UI code. The offline path remains runnable without the worker.

## Implementation Sequence

1. Add entities, scan-state fields, relationships, indexes, and notification preference support.
2. Add storage, scan, and document service interfaces; implement local storage and deterministic local scan behavior.
3. Add durable upload sequencing, queue message/outbox publication, idempotency, retry, quarantine, and scan-state transitions.
4. Add the Azure Functions Queue Storage trigger and production scanner adapter without introducing it into the offline runtime path.
5. Add authorized preview/download endpoints and audit activity recording.
6. Add document listing, per-file metadata upload, management, sharing, and Shared with Me views.
7. Integrate project, task, dashboard, notification, and administrator reporting workflows.
8. Add focused tests and run the local plus production-like queue validation scenarios.

## Complexity Tracking

No constitutional violations identified. The Azure Functions worker is justified by the explicit asynchronous scanning requirement and remains isolated from the offline training runtime.
