namespace UavFleetManagement.Application.DTOs.Storage;

public record PresignedUploadRequestDto(
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string EntityType,
    string EntityId
);

public record PresignedUploadResponseDto(
    Guid FileId,
    string UploadUrl,
    string StorageKey,
    string BucketName,
    DateTime ExpiresAt
);

public record CommitUploadRequestDto(
    Guid FileId,
    string StorageKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Checksum,
    string EntityType,
    string EntityId
);

public record FileAttachmentDto(
    Guid FileId,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string Sha256Checksum,
    string EntityType,
    string EntityId,
    string Status,
    string? DownloadUrl,
    DateTime CreatedAt
);
