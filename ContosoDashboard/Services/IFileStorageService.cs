namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string originalFileName, string contentType, int userId, int? projectId, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string storedFilePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storedFilePath, CancellationToken cancellationToken = default);
    Task QuarantineAsync(string storedFilePath, CancellationToken cancellationToken = default);
}
