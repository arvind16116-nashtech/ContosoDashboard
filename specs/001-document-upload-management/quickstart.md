# Quickstart: Document Upload and Management

## Local setup

1. Open the solution in Visual Studio or VS Code.
2. Confirm the existing SQL Server connection string is configured in `appsettings.Development.json`.
3. Restore NuGet packages with `dotnet restore`.
4. Build the app with `dotnet build`.

## Run the app

```bash
dotnet run --project ContosoDashboard/ContosoDashboard.csproj
```

## Validate the feature

1. Sign in using the seeded user accounts.
2. Navigate to the dashboard or project page.
3. Upload a valid PDF or Office document under 25 MB.
4. Confirm the file appears in the user’s list and project page.
5. Attempt an unsupported file type to confirm validation.
6. Share the file with another user and verify the in-app notification is created.

## Security checks

- Ensure files are stored outside `wwwroot`.
- Confirm the file name is GUID-based and not user-controlled.
- Verify protected access is enforced through service-layer authorization.
