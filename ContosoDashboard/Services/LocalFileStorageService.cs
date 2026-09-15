using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public sealed class FileStorageOptions
{
    public string RootPath { get; set; } = "AppData/uploads";
}

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<FileStorageOptions> options, IWebHostEnvironment environment)
    {
        _rootPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.RootPath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string originalFileName, string contentType, int userId, int? projectId, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var scope = projectId?.ToString() ?? "personal";
        var relativePath = Path.Combine(userId.ToString(), scope, $"{Guid.NewGuid():N}{extension}");
        var fullPath = ResolvePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true);
        await fileStream.CopyToAsync(output, cancellationToken);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    public Task<Stream?> DownloadAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storedFilePath);
        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storedFilePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public async Task QuarantineAsync(string storedFilePath, CancellationToken cancellationToken = default)
    {
        var sourcePath = ResolvePath(storedFilePath);
        if (!File.Exists(sourcePath)) return;
        var quarantinePath = ResolvePath(Path.Combine("quarantine", Path.GetFileName(storedFilePath)));
        Directory.CreateDirectory(Path.GetDirectoryName(quarantinePath)!);
        await Task.Run(() => File.Move(sourcePath, quarantinePath, overwrite: true), cancellationToken);
    }

    private string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new InvalidOperationException("A relative storage path is required.");

        var combined = Path.GetFullPath(Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar) ? _rootPath : _rootPath + Path.DirectorySeparatorChar;
        if (!combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The storage path is outside the configured storage root.");
        return combined;
    }
}
