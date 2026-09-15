# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-document-upload-management/spec.md`

## Summary

This feature adds a secure upload-and-search document workflow to the existing Blazor Server dashboard. The implementation should preserve the current layered architecture, insert a filesystem abstraction for future cloud migration, and enforce role- and project-based authorization before any file is downloaded or shared.

## Technical Context

**Language/Version**: C# on .NET 8 / ASP.NET Core 8  
**Primary Dependencies**: Blazor Server, EF Core, SQL Server LocalDB/SQL Server, ASP.NET Core Authentication & Authorization  
**Storage**: Local filesystem storage under AppData/uploads with metadata persisted in the existing SQL database  
**Testing**: xUnit or equivalent .NET test project if added for validation; UI plus service-level verification for this feature  
**Target Platform**: Windows development environment with web app deployment to ASP.NET Core host  
**Project Type**: Single web application with Blazor Server UI and centralized services  
**Performance Goals**: Document list and search within 2 seconds for up to 500 documents; upload under 30 seconds for 25 MB files on typical network  
**Constraints**: Offline/local-first training environment; no cloud dependency; maintain mock-auth pattern; no major rewrite of the current app structure  
**Scale/Scope**: Small internal enterprise dashboard supporting project members, managers, and administrators

## Constitution Check

Status: Passes with repository constraints.

- The feature remains within the current application architecture: Blazor Server + EF Core + SQL Server + mock auth.
- Security requirements align with the existing app’s authorization model and role policies.
- No conflict with current repository guidance; the design uses local storage and interface-based abstractions for future migration.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-upload-management/
├── spec.md              # Feature requirements and user stories
├── plan.md              # Technical implementation plan
├── research.md          # Architecture decisions and trade-offs
├── data-model.md        # Document entities and relationships
├── quickstart.md        # Setup and validation steps
├── contracts/           # API/data contracts for upload and sharing
├── tasks.md             # Executable implementation plan
└── README.md            # Optional feature summary
```

### Source Code (repository root)

```text
ContosoDashboard/
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── User.cs
│   ├── Project.cs
│   ├── Notification.cs
│   ├── Document.cs
│   ├── DocumentShare.cs
│   └── ...
├── Pages/
│   ├── Index.razor
│   ├── Projects.razor
│   ├── ProjectDetails.razor
│   ├── Tasks.razor
│   └── ...
├── Services/
│   ├── DashboardService.cs
│   ├── NotificationService.cs
│   ├── ProjectService.cs
│   ├── TaskService.cs
│   ├── UserService.cs
│   ├── IFileStorageService.cs
│   ├── LocalFileStorageService.cs
│   └── DocumentService.cs
├── Shared/
│   ├── MainLayout.razor
│   └── NavMenu.razor
├── wwwroot/
│   └── css/
├── Program.cs
└── appsettings*.json
```

**Structure Decision**: Keep the feature within the existing single-project Blazor Server app, extending the current `Models`, `Data`, `Services`, and `Pages` folders rather than creating a separate backend/frontend split.

## Research and Design Notes

- Use `AppData/uploads` outside `wwwroot` for stored files to prevent direct browser serving.
- Add `IFileStorageService` with task-specific methods for upload, delete, download, and URL generation.
- Generate unique GUID-based names before writing to disk to avoid duplicate path errors and path traversal risk.
- Model `Document` keys as integers, matching existing `User` and `Project` IDs.
- Keep categories as string values in the database to preserve product simplicity and minimize migration risk.
- Add project membership and share checks in the service layer so data access remains centralized and auditable.
- Extend `ApplicationDbContext` with `DbSet<Document>` and `DbSet<DocumentShare>` plus indexes for project, uploader, and category lookups.

## Complexity Tracking

No constitutional violations identified. The design intentionally avoids adding a second project or introducing a new framework to stay aligned with the app’s scope and training goals.
