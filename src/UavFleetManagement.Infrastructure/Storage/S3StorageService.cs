using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UavFleetManagement.Application.Common.Interfaces;

namespace UavFleetManagement.Infrastructure.Storage;

public class S3StorageService : IS3StorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3StorageService> _logger;
    private readonly string _localStorageRoot;

    public S3StorageService(IConfiguration configuration, ILogger<S3StorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _localStorageRoot = Path.Combine(AppContext.BaseDirectory, "App_Data", "s3_storage");

        if (!Directory.Exists(_localStorageRoot))
        {
            Directory.CreateDirectory(_localStorageRoot);
        }
    }

    public Task<string> GeneratePresignedUploadUrlAsync(string bucketName, string key, string contentType, int expiryMinutes = 15)
    {
        // В локальном режиме возвращаем прямой upload endpoint
        var baseUrl = _configuration["AppBaseUrl"] ?? "http://localhost:5000";
        var safeKey = Uri.EscapeDataString(key);
        var url = $"{baseUrl}/api/storage/upload-direct?bucket={bucketName}&key={safeKey}&expires={DateTime.UtcNow.AddMinutes(expiryMinutes):O}";
        return Task.FromResult(url);
    }

    public Task<string> GeneratePresignedDownloadUrlAsync(string bucketName, string key, int expiryMinutes = 60)
    {
        var baseUrl = _configuration["AppBaseUrl"] ?? "http://localhost:5000";
        var safeKey = Uri.EscapeDataString(key);
        var url = $"{baseUrl}/api/storage/files/{bucketName}/{safeKey}";
        return Task.FromResult(url);
    }

    public async Task<bool> UploadFileAsync(string bucketName, string key, Stream contentStream, string contentType)
    {
        try
        {
            var targetDir = Path.Combine(_localStorageRoot, bucketName, Path.GetDirectoryName(key) ?? string.Empty);
            Directory.CreateDirectory(targetDir);

            var filePath = Path.Combine(_localStorageRoot, bucketName, key);
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await contentStream.CopyToAsync(fileStream);

            _logger.LogInformation("Файл успешно сохранен в S3-хранилище: {Bucket}/{Key}", bucketName, key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке файла в хранилище {Bucket}/{Key}", bucketName, key);
            return false;
        }
    }

    public Task<Stream?> DownloadFileAsync(string bucketName, string key)
    {
        var filePath = Path.Combine(_localStorageRoot, bucketName, key);
        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteFileAsync(string bucketName, string key)
    {
        var filePath = Path.Combine(_localStorageRoot, bucketName, key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
