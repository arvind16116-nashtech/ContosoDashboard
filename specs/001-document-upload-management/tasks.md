# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Format: `[ID] [P] [Story] Description`

## Phase 1: Setup and Shared Infrastructure

**Purpose**: Establish the document storage and data model foundation before feature work begins.

- [ ] T001 [P] [Setup] Add the document entity and share model definitions in `ContosoDashboard/Models/Document.cs` and `ContosoDashboard/Models/DocumentShare.cs`
- [ ] T002 [P] [Setup] Extend `ContosoDashboard/Data/ApplicationDbContext.cs` to include `DbSet<Document>` and `DbSet<DocumentShare>` plus indexes and seed data support
- [ ] T003 [P] [Setup] Create the storage abstraction contract in `ContosoDashboard/Services/IFileStorageService.cs`
- [ ] T004 [P] [Setup] Implement the local filesystem provider in `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T005 [Setup] Add document-related configuration and local upload directory setup in `ContosoDashboard/appsettings*.json` and `ContosoDashboard/Program.cs`
- [ ] T006 [Setup] Create `ContosoDashboard/Services/DocumentService.cs` with core authorization and validation logic for uploads, downloads, and deletes

**Checkpoint**: The shared model, database access, and secure storage layer are ready.

---

## Phase 2: User Story 1 - Secure document upload and metadata capture (Priority: P1) 🎯 MVP

**Goal**: Allow authenticated users to upload supported documents with metadata and secure storage.

**Independent Test**: A logged-in user can upload a file, view it in their document list, and see validation errors when the file is invalid.

### Implementation for User Story 1

- [ ] T007 [P] [US1] Add a `Document` upload request model and validation helpers in `ContosoDashboard/Models/` or a dedicated request DTO file if needed
- [ ] T008 [US1] Implement file validation in `ContosoDashboard/Services/DocumentService.cs` for file type, size, and path safety checks
- [ ] T009 [US1] Implement upload flow in `ContosoDashboard/Services/DocumentService.cs` to generate GUID names, save to disk, and persist metadata to the database
- [ ] T010 [US1] Register the document services in `ContosoDashboard/Program.cs` and support dependency injection for the file storage abstraction
- [ ] T011 [US1] Add an upload page or component for multiple file selection and metadata entry in `ContosoDashboard/Pages/`
- [ ] T012 [US1] Add success and error messaging in the UI and ensure progress feedback is visible during upload
- [ ] T013 [US1] Add a personal documents view that lists uploaded items with title, category, date, file size, and project association in `ContosoDashboard/Pages/`

**Checkpoint**: User Story 1 is complete and independently testable as a functioning document upload experience.

---

## Phase 3: User Story 2 - Search, browse, and manage document access by project and role (Priority: P2)

**Goal**: Make documents discoverable and enforce access rules for project and role-based visibility.

**Independent Test**: A user can browse, filter, sort, and search documents and only sees items they are allowed to access.

### Implementation for User Story 2

- [ ] T014 [P] [US2] Add filtering, sorting, and project-specific document query support in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T015 [US2] Add search by title, description, tags, uploader, and project in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T016 [US2] Add project document listing logic and authorization checks in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T017 [US2] Create or update project-detail document sections in `ContosoDashboard/Pages/ProjectDetails.razor` and related page markup
- [ ] T018 [US2] Add download and preview authorization checks and secure file-serving behavior in the document service and page flow
- [ ] T019 [US2] Add edit metadata and replace-file workflows to the document management UI in `ContosoDashboard/Pages/`
- [ ] T020 [US2] Implement delete confirmation and cleanup logic for file and metadata removal in `ContosoDashboard/Services/DocumentService.cs`

**Checkpoint**: Users can browse and access project documents without exposing unauthorized files.

---

## Phase 4: User Story 3 - Sharing, notifications, and dashboard integration (Priority: P3)

**Goal**: Deliver collaboration features and project/task awareness for document activities.

**Independent Test**: A user can share a document, receive a notification, and see document count and recent documents on the dashboard or task page.

### Implementation for User Story 3

- [ ] T021 [P] [US3] Extend `ContosoDashboard/Models/DocumentShare.cs` usage and add share-related service methods in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T022 [US3] Add notification triggers for document sharing and new project document events in `ContosoDashboard/Services/NotificationService.cs`
- [ ] T023 [US3] Add shared-with-me query support to document retrieval and list views in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T024 [US3] Update `ContosoDashboard/Pages/Index.razor` with a recent documents widget and document count summary integration
- [ ] T025 [US3] Update task views in `ContosoDashboard/Pages/Tasks.razor` or task detail sections to show related documents and allow attachment from the task flow
- [ ] T026 [US3] Add document access logging and audit tracking in `ContosoDashboard/Services/DocumentService.cs` and supporting notification or reporting code

**Checkpoint**: Sharing, notifications, and dashboard visibility are present and consistent across the app.

---

## Phase 5: Validation, polish, and security hardening

**Purpose**: Verify behavior against the feature requirements and tighten security and UX.

- [ ] T027 [P] Validate upload, search, and authorization behavior against the scenarios in `specs/001-document-upload-management/spec.md`
- [ ] T028 [P] Confirm the local storage path is outside `wwwroot` and all generated paths are GUID-based and safe
- [ ] T029 [P] Test the file size, unsupported extension, and failed-save error paths end to end
- [ ] T030 [P] Review dashboard and task UI flows for missing notifications, counts, or permission mismatches
- [ ] T031 [P] Update any documentation required to reflect the new document workflow in the project docs

---

## Dependencies & Execution Order

- Phase 1 must complete before any user story work begins.
- User Story 1 is the MVP and should be verified before moving to later stories.
- User Story 2 depends on the upload and authorization foundation from User Story 1.
- User Story 3 depends on both the upload and project visibility features being stable.
- Final validation should run after all user-story tasks are complete.
