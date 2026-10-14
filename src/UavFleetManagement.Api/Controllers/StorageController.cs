using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Storage;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Entities.Storage;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IS3StorageService _storageService;

    public StorageController(IApplicationDbContext context, IS3StorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    /// <summary>
    /// Получение Presigned URL для прямой загрузки файла в S3
    /// </summary>
    [HttpPost("attachments/presign")]
    public async Task<IActionResult> GeneratePresignedUpload([FromBody] PresignedUploadRequestDto dto)
    {
        var fileId = Guid.NewGuid();
        var ext = Path.GetExtension(dto.FileName);
        var key = $"{dto.EntityType.ToLower()}/{DateTime.UtcNow:yyyy/MM}/{fileId}{ext}";
        const string bucket = "uav-documents";

        var uploadUrl = await _storageService.GeneratePresignedUploadUrlAsync(bucket, key, dto.ContentType, 15);

        return Ok(new PresignedUploadResponseDto(
            FileId: fileId,
            UploadUrl: uploadUrl,
            StorageKey: key,
            BucketName: bucket,
            ExpiresAt: DateTime.UtcNow.AddMinutes(15)
        ));
    }

    /// <summary>
    /// Прямая загрузка файла через API (локальный fallback для S3/MinIO)
    /// </summary>
    [HttpPost("upload-direct")]
    public async Task<IActionResult> UploadDirect([FromQuery] string bucket, [FromQuery] string key, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Файл не передан.");
        }

        await using var stream = file.OpenReadStream();
        bool success = await _storageService.UploadFileAsync(bucket, key, stream, file.ContentType);

        if (!success)
        {
            return StatusCode(500, "Ошибка сохранения файла в S3 хранилище.");
        }

        return Ok(new { success = true, bucket, key, size = file.Length });
    }

    /// <summary>
    /// Фиксация загруженного файла и сохранение метаданных в БД (Транзакционный Outbox)
    /// </summary>
    [HttpPost("attachments/commit")]
    public async Task<IActionResult> CommitUpload([FromBody] CommitUploadRequestDto dto)
    {
        var attachment = new FileAttachment
        {
            FileId = dto.FileId,
            BucketName = "uav-documents",
            StorageKey = dto.StorageKey,
            OriginalFileName = dto.OriginalFileName,
            ContentType = dto.ContentType,
            FileSizeBytes = dto.FileSizeBytes,
            Sha256Checksum = dto.Sha256Checksum,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Status = "PendingVerification",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FileAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        // Transactional Outbox: событие загрузки файла для антивирусной проверки и генерации превью
        var outboxEvent = new OutboxEvent
        {
            AggregateType = "Storage",
            AggregateId = attachment.FileId.ToString(),
            EventType = "FileAttachmentUploadedEvent",
            Payload = JsonSerializer.Serialize(new
            {
                fileId = attachment.FileId,
                bucketName = attachment.BucketName,
                storageKey = attachment.StorageKey,
                originalFileName = attachment.OriginalFileName,
                fileSizeBytes = attachment.FileSizeBytes,
                sha256Checksum = attachment.Sha256Checksum,
                entityType = attachment.EntityType,
                entityId = attachment.EntityId
            }),
            Metadata = JsonSerializer.Serialize(new
            {
                traceId = HttpContext.TraceIdentifier,
                userId = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            }),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync();

        var downloadUrl = await _storageService.GeneratePresignedDownloadUrlAsync(attachment.BucketName, attachment.StorageKey);

        return Ok(new FileAttachmentDto(
            attachment.FileId,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.Sha256Checksum,
            attachment.EntityType,
            attachment.EntityId,
            attachment.Status,
            downloadUrl,
            attachment.CreatedAt
        ));
    }

    /// <summary>
    /// Получение списка файлов, прикрепленных к сущности
    /// </summary>
    [HttpGet("attachments")]
    public async Task<IActionResult> GetAttachments([FromQuery] string entityType, [FromQuery] string entityId)
    {
        var files = await _context.FileAttachments
            .Where(f => f.EntityType == entityType && f.EntityId == entityId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var result = new List<FileAttachmentDto>();
        foreach (var f in files)
        {
            var downloadUrl = await _storageService.GeneratePresignedDownloadUrlAsync(f.BucketName, f.StorageKey);
            result.Add(new FileAttachmentDto(
                f.FileId,
                f.OriginalFileName,
                f.ContentType,
                f.FileSizeBytes,
                f.Sha256Checksum,
                f.EntityType,
                f.EntityId,
                f.Status,
                downloadUrl,
                f.CreatedAt
            ));
        }

        return Ok(result);
    }

    /// <summary>
    /// Скачивание бинарного файла из S3-хранилища
    /// </summary>
    [HttpGet("files/{bucket}/{*key}")]
    public async Task<IActionResult> DownloadFile(string bucket, string key)
    {
        var stream = await _storageService.DownloadFileAsync(bucket, key);
        if (stream == null)
        {
            return NotFound("Файл в S3 хранилище не найден.");
        }

        return File(stream, "application/octet-stream", Path.GetFileName(key));
    }
}
