namespace UavFleetManagement.Domain.Entities.Storage;

public class FileAttachment
{
    public Guid FileId { get; set; } = Guid.NewGuid();
    public string BucketName { get; set; } = "uav-documents";
    public string StorageKey { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSizeBytes { get; set; }
    public string Sha256Checksum { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public int? UploadedByUserId { get; set; }
    public string Status { get; set; } = "PendingVerification";
    public string? StorageUrl { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
