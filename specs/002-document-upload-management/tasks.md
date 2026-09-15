# Tasks: Document Upload and Management

**Input**: Design documents from `specs/002-document-upload-management/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/document-management-contract.md`, `quickstart.md`

**Organization**: Tasks are grouped by user story so each increment can be implemented and validated independently after the foundational phase.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish the project and deployment structure required by the feature.

- [X] T001 Create the production worker project structure under `DocumentScanWorker/` with Azure Functions isolated-worker configuration and a placeholder `Program.cs`.
- [X] T002 [P] Add the `DocumentScanWorker/DocumentScanWorker.csproj` project file targeting the supported .NET version and Azure Functions worker packages without adding those dependencies to the offline web project.
- [X] T003 [P] Add local and production configuration placeholders for storage roots, queue names, scan mode, and retry settings in `ContosoDashboard/appsettings.json` and `DocumentScanWorker/local.settings.json`.
- [X] T004 [P] Add shared scan-message contract types under `DocumentScanWorker/Contracts/DocumentScanRequested.cs` with schema version, document ID, storage key, content type, size, message ID, and correlation ID.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement the data, security, storage, and scan-state foundations required by all user stories.

**Checkpoint**: Foundation ready; user-story work can begin after these tasks complete.

- [X] T005 Add `Document`, `DocumentShare`, `DocumentActivity`, and scan-state properties to `ContosoDashboard/Models/Document.cs`, `ContosoDashboard/Models/DocumentShare.cs`, and `ContosoDashboard/Models/DocumentActivity.cs` using integer keys, text categories, 255-character MIME storage, and pending/clean/infected/failed/quarantined states.
- [X] T006 Update `ContosoDashboard/Data/ApplicationDbContext.cs` with document DbSets, relationships to `User`, `Project`, and `TaskItem`, indexes for accessible search/list queries, uniqueness for scan correlation IDs, and seeded notification preference defaults.
- [X] T007 [P] Add `ContosoDashboard/Services/IFileStorageService.cs` for opaque upload, download, delete, and quarantine operations without exposing physical roots.
- [X] T008 [P] Add `ContosoDashboard/Services/IDocumentScanService.cs` and `ContosoDashboard/Services/DocumentScanRequested.cs` for local deterministic validation and versioned production scan handoff contracts.
- [X] T009 Implement `ContosoDashboard/Services/LocalFileStorageService.cs` with a configured root outside `wwwroot`, GUID-based paths, traversal/rooted-path rejection, stream disposal safety, and cleanup of failed writes.
- [X] T010 Implement `ContosoDashboard/Services/LocalDocumentScanService.cs` with the offline type, extension, MIME, and 25 MB validation behavior defined in `specs/002-document-upload-management/spec.md`.
- [X] T011 Add `ContosoDashboard/Services/DocumentAuthorizationService.cs` to centralize ownership, project membership, team-lead resource access, project-manager access, administrator access, active shares, notification preference checks, and scan-state availability checks.
- [X] T012 Add `ContosoDashboard/Services/IDocumentService.cs` and the `DocumentService` skeleton in `ContosoDashboard/Services/DocumentService.cs`, including controlled failure results that do not disclose unauthorized document existence.
- [X] T013 Register document services, local storage, local scan behavior, and configuration binding in `ContosoDashboard/Program.cs` while preserving the existing SQLite provider and mock authorization policies.
- [X] T014 Add the authorized stream boundary in `ContosoDashboard/Controllers/DocumentController.cs` for preview and download, enforcing clean scan state and authorization before opening any file outside `wwwroot`.
- [X] T015 Add scan queue publication abstractions and durable retry state in `ContosoDashboard/Services/IDocumentScanQueue.cs`, `ContosoDashboard/Services/DocumentScanQueue.cs`, and the document schema so queue failures leave documents unavailable and retryable.
- [X] T016 Add shared Azure worker references and configuration documentation in `DocumentScanWorker/README.md`, including Queue Storage connection settings, queue name, poison-message behavior, and the rule that queue messages never contain file bytes or physical paths.

---

## Phase 3: User Story 1 - Upload and Organize a Document (Priority: P1) 🎯 MVP

**Goal**: Allow employees to upload supported files with independent metadata, private storage, clear feedback, and safe scan-state handling.

**Independent Test**: An authenticated employee can upload one or more valid files with per-file title and category, see each document in their list, and receive clear rejection messages for invalid files.

### Implementation for User Story 1

- [X] T017 [US1] Implement per-file upload validation and metadata mapping in `ContosoDashboard/Services/DocumentService.cs` for required title/category, optional description/project/tags, supported file types, and 25 MB maximum size.
- [X] T018 [US1] Implement the upload sequence in `ContosoDashboard/Services/DocumentService.cs`: authorize project/task association, generate an opaque path, save the file, persist pending metadata, publish or record the scan request, and clean up on failure.
- [X] T019 [US1] Add upload progress, per-file metadata, validation messages, success/failure states, and `@key`-based file input reset to `ContosoDashboard/Pages/Documents.razor`.
- [X] T020 [US1] Add document list rendering for the current user’s uploaded metadata in `ContosoDashboard/Pages/Documents.razor`, excluding pending/infected/quarantined documents from accessible results.
- [X] T021 [US1] Add audit activity creation for accepted uploads and rejected upload attempts in `ContosoDashboard/Services/DocumentService.cs` without storing file contents or secrets.
- [X] T022 [US1] Add document navigation and protected-page authorization for `ContosoDashboard/Pages/Documents.razor` in `ContosoDashboard/Shared/NavMenu.razor` and the page itself.

**Checkpoint**: User Story 1 is independently usable as the MVP after the foundational phase.

---

## Phase 4: User Story 2 - Browse, Search, Preview, and Download Documents (Priority: P1)

**Goal**: Let users find and retrieve only documents they are authorized to access.

**Independent Test**: A user can search, sort, filter, preview a permitted PDF/image, and download a permitted file while unauthorized identifiers return access denied without file disclosure.

- [X] T023 [US2] Implement permission-scoped list, search, sort, category/project/date filters, and pagination in `ContosoDashboard/Services/DocumentService.cs` for title, description, tags, uploader, project, date, and size criteria.
- [X] T024 [US2] Add search, sort, filter, loading, empty, and error states to `ContosoDashboard/Pages/Documents.razor` with stable list dimensions and response feedback.
- [X] T025 [US2] Add the Shared with Me page in `ContosoDashboard/Pages/SharedDocuments.razor` using active-share authorization and excluding unavailable scan states.
- [X] T026 [US2] Implement preview and download response handling in `ContosoDashboard/Controllers/DocumentController.cs` with content-type allowlisting for PDF/images, safe download names, and activity recording.
- [X] T027 [US2] Add authorized preview/download controls and error handling to `ContosoDashboard/Pages/Documents.razor` and `ContosoDashboard/Pages/SharedDocuments.razor`.
- [X] T028 [US2] Add query indexes and projection updates in `ContosoDashboard/Data/ApplicationDbContext.cs` required to meet list/search targets for up to 500 accessible documents.

**Checkpoint**: User Stories 1 and 2 are independently usable after this phase.

---

## Phase 5: User Story 3 - Manage and Share Documents (Priority: P2)

**Goal**: Allow owners and authorized project managers to update, replace, delete, and share documents within defined privacy boundaries.

**Independent Test**: An owner can edit metadata, replace a file, delete it after confirmation, and share valid recipients; project documents cannot be shared outside project membership, and personal documents require explicit sharing.

- [X] T029 [US3] Implement metadata update authorization and validation in `ContosoDashboard/Services/DocumentService.cs` for owners, project managers, team leads, and administrators according to category and project rules.
- [X] T030 [US3] Implement replacement sequencing in `ContosoDashboard/Services/DocumentService.cs` so the prior clean file remains available until the replacement is validated/stored and a failed replacement preserves the prior version.
- [X] T031 [US3] Implement permanent deletion and stored-file cleanup in `ContosoDashboard/Services/DocumentService.cs`, including unavailable state transition and deletion activity recording.
- [X] T032 [US3] Implement user/team sharing validation in `ContosoDashboard/Services/DocumentService.cs`, restricting project documents to existing project members and personal documents to active recipients.
- [X] T033 [US3] Integrate configurable document notification delivery in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/NotificationService.cs`, suppressing both direct-share and project-document notifications when disabled without changing access.
- [X] T034 [US3] Add metadata edit, replacement, delete confirmation, share recipient selection, and notification-preference-aware feedback to `ContosoDashboard/Pages/Documents.razor`.
- [X] T035 [US3] Add document notification preference controls or integration to `ContosoDashboard/Pages/Profile.razor` and persist the preference through the existing user service/model.

**Checkpoint**: User Story 3 is independently usable with secure ownership, project, sharing, and notification behavior.

---

## Phase 6: User Story 4 - Use Documents in Projects, Tasks, and the Dashboard (Priority: P2)

**Goal**: Integrate documents into existing project, task, dashboard, and notification workflows.

**Independent Test**: Project members can view project documents, task users can attach/upload documents associated with the task’s project, and the dashboard shows five recent uploads plus count.

- [X] T036 [US4] Add project-scoped document listing and permitted project-manager upload/manage actions to `ContosoDashboard/Pages/ProjectDetails.razor` using `DocumentService` authorization.
- [X] T037 [US4] Add task document listing and attach/upload actions to `ContosoDashboard/Pages/Tasks.razor`, enforcing that task and document project associations match.
- [X] T038 [US4] Extend `ContosoDashboard/Services/IDashboardService.cs` and `ContosoDashboard/Services/DashboardService.cs` with the current user’s five recent documents and total accessible document count.
- [X] T039 [US4] Add the Recent Documents widget and document count summary to `ContosoDashboard/Pages/Index.razor` with loading and empty states.
- [X] T040 [US4] Add project-document notification event integration to `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/NotificationService.cs`, honoring disabled document notifications.
- [X] T041 [US4] Update `ContosoDashboard/Pages/ProjectDetails.razor`, `ContosoDashboard/Pages/Tasks.razor`, and `ContosoDashboard/Pages/Index.razor` for responsive document actions without exposing storage paths or unauthorized records.

**Checkpoint**: User Story 4 is independently demonstrable from project, task, and dashboard workflows.

---

## Phase 7: User Story 5 - Review Document Activity (Priority: P3)

**Goal**: Give administrators auditable document activity and usage summaries while denying non-administrators.

**Independent Test**: An administrator can view activity and reports for uploads, scans, downloads, deletions, replacements, and shares; non-administrators are denied.

- [X] T042 [US5] Implement administrator-only activity and aggregate report queries in `ContosoDashboard/Services/DocumentService.cs` for document types, uploaders, access patterns, and scan outcomes.
- [X] T043 [US5] Add administrator authorization and report rendering to `ContosoDashboard/Pages/DocumentActivity.razor`, including safe operational visibility for failed/poisoned scan requests.
- [X] T044 [US5] Add audit entries for preview, download, update, replace, delete, share, scan clean, scan infected, scan failure, and quarantine transitions in `ContosoDashboard/Services/DocumentService.cs` and worker result handling.

**Checkpoint**: User Story 5 is independently usable by administrators and inaccessible to other roles.

---

## Phase 8: Production Async Virus Scanning

**Purpose**: Add the production-only Azure Functions Queue Storage background job without changing the offline training path.

- [X] T045 Implement `DocumentScanWorker/Functions/DocumentScanQueueFunction.cs` with a Queue Storage trigger, schema-version validation, correlation logging, and idempotent document scan processing.
- [ ] T046 [P] Implement `DocumentScanWorker/Services/IMalwareScanner.cs` and `DocumentScanWorker/Services/AzureMalwareScanner.cs` with configurable scanner integration, safe result mapping, and no file-content logging.
- [ ] T047 Implement `DocumentScanWorker/Services/ScanResultHandler.cs` to transition pending documents to Clean, Infected, Failed, or Quarantined, enforce terminal-state idempotency, and record audit activity.
- [ ] T048 Implement queue retry and poison-message handling configuration in `DocumentScanWorker/host.json` and `DocumentScanWorker/Program.cs`, leaving unrecoverable documents unavailable and operationally visible.
- [ ] T049 Add the production storage/queue adapter in `ContosoDashboard/Services/AzureDocumentScanQueue.cs` and configuration-based registration in `ContosoDashboard/Program.cs`, keeping Azure services disabled for local training.
- [ ] T050 Add reconciliation/outbox retry processing in `ContosoDashboard/Services/DocumentScanQueue.cs` so metadata committed without a published message is republished without making the document available.
- [ ] T051 Document Azure deployment, Queue Storage settings, scanner configuration, identity permissions, and local-vs-production behavior in `DocumentScanWorker/README.md` and `README.md`.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Complete evidence-based validation, security review, performance checks, and documentation.

- [ ] T052 [P] Update `README.md` with document feature behavior, offline validation, private file storage, role boundaries, and Azure Functions production scanning architecture.
- [ ] T053 [P] Review `ContosoDashboard/wwwroot/css/site.css` for accessible document tables, upload feedback, preview controls, responsive layout, and no storage-path disclosure.
- [ ] T054 Run `dotnet restore .\ContosoDashboard\ContosoDashboard.csproj` and record the result in the feature validation notes at `specs/002-document-upload-management/quickstart.md`.
- [ ] T055 Run `dotnet build .\ContosoDashboard\ContosoDashboard.csproj --no-restore` and resolve feature-caused compile errors before completion.
- [ ] T056 Run the offline scenarios in `specs/002-document-upload-management/quickstart.md`, including invalid uploads, authorization/IDOR checks, replacement failure, deletion cleanup, sharing, and disabled notifications.
- [ ] T057 Run the production-like Queue Storage scenarios in `specs/002-document-upload-management/quickstart.md`, including clean, infected, transient retry, poison-message, duplicate-delivery, and queue-publication-failure cases.
- [ ] T058 Review all document endpoints and service queries for authorization-before-disclosure, scan-state gating, path traversal prevention, and safe error messages in `ContosoDashboard/Services/DocumentAuthorizationService.cs`, `ContosoDashboard/Services/DocumentService.cs`, and `ContosoDashboard/Controllers/DocumentController.cs`.
- [ ] T059 Measure upload, list, search, and preview targets using the scenarios in `specs/002-document-upload-management/quickstart.md` and document any residual performance gaps.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: No dependencies; tasks T001-T004 can run in parallel where files differ.
- **Phase 2 Foundational**: Depends on Phase 1; blocks all user-story implementation.
- **Phase 3 User Story 1**: Depends on Phase 2; MVP upload slice.
- **Phase 4 User Story 2**: Depends on Phase 2 and the document service from Phase 3 for shared list/upload surfaces.
- **Phase 5 User Story 3**: Depends on Phase 2 and the document service from Phase 3; extends upload and access rules.
- **Phase 6 User Story 4**: Depends on Phase 2 and the document service from Phase 3; integrates with existing pages.
- **Phase 7 User Story 5**: Depends on activity recording from Phases 3-6.
- **Phase 8 Async Virus Scanning**: Depends on Phase 2 queue contracts and Phase 3 pending-state upload workflow; production-only deployment.
- **Phase 9 Polish**: Depends on all desired stories and the async scanning path.

### User Story Completion Order

- **US1 (P1)**: First MVP; depends on foundational storage, models, authorization, and local scan behavior.
- **US2 (P1)**: Can proceed after foundational work, with shared document query/service surfaces from US1.
- **US3 (P2)**: Depends on the upload and access model from US1/US2.
- **US4 (P2)**: Depends on document service contracts and existing project/task/dashboard pages.
- **US5 (P3)**: Depends on activity records emitted by earlier stories.
- **Async scan worker**: Can be developed in parallel with UI stories after foundational queue/state contracts, but production validation depends on the upload state machine.

### Parallel Opportunities

- Phase 1: T002, T003, and T004 can run in parallel after T001 establishes the worker directory.
- Phase 2: T007, T008, and T016 can run in parallel; T009 and T010 can then proceed independently before T012/T015 integration.
- After Phase 2: US2, US3, US4, and the worker can be assigned in parallel once their shared service contracts are stable.
- Within US3: metadata, replacement/deletion, sharing, and profile preference work can be split by file ownership after T012.
- Within Phase 8: T046 and T048 can proceed in parallel with the queue function skeleton T045.

## Parallel Example: User Story 1

```text
Task: T017 Implement per-file upload validation in ContosoDashboard/Services/DocumentService.cs
Task: T019 Add upload UI and per-file metadata in ContosoDashboard/Pages/Documents.razor
Task: T022 Add protected document navigation in ContosoDashboard/Shared/NavMenu.razor
```

## Parallel Example: Async Scanning

```text
Task: T045 Implement the Azure Functions Queue Storage trigger in DocumentScanWorker/Functions/DocumentScanQueueFunction.cs
Task: T046 Implement the malware scanner adapter in DocumentScanWorker/Services/AzureMalwareScanner.cs
Task: T048 Configure retries and poison-message handling in DocumentScanWorker/host.json
```

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 Setup.
2. Complete Phase 2 Foundational infrastructure.
3. Complete Phase 3 User Story 1.
4. Validate supported/unsupported uploads, per-file metadata, private storage, pending/local-clean behavior, and clear feedback.
5. Stop for an MVP demonstration before adding browse, sharing, integrations, or production worker deployment.

### Incremental Delivery

1. Add US1 upload and organization.
2. Add US2 browsing, search, preview, and download.
3. Add US3 management, sharing, and notification preferences.
4. Add US4 project/task/dashboard integration.
5. Add US5 administration and reporting.
6. Add the production-only Azure Functions scanning worker and queue recovery path.
7. Run all offline and production-like quickstart validations.

### Notes

- Every task uses the required checklist format: checkbox, sequential ID, optional `[P]`, required story label in story phases, and an explicit file path.
- The production Azure Functions worker must never be required to build or run the offline training application.
- Direct access remains denied until a document has a clean scan state in production or the deterministic local validation result in training.
