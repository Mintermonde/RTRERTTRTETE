namespace UavFleetManagement.Application.Common.Interfaces;

public interface IS3StorageService
{
    Task<string> GeneratePresignedUploadUrlAsync(string bucketName, string key, string contentType, int expiryMinutes = 15);
    Task<string> GeneratePresignedDownloadUrlAsync(string bucketName, string key, int expiryMinutes = 60);
    Task<bool> UploadFileAsync(string bucketName, string key, Stream contentStream, string contentType);
    Task<Stream?> DownloadFileAsync(string bucketName, string key);
    Task<bool> DeleteFileAsync(string bucketName, string key);
}
