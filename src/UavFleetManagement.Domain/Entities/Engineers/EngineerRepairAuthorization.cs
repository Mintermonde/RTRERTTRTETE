using UavFleetManagement.Domain.Entities.Catalog;

namespace UavFleetManagement.Domain.Entities.Engineers;

public class EngineerRepairAuthorization
{
    public int AuthId { get; set; }
    public int EngineerId { get; set; }
    public int? AllowedUavModelId { get; set; }
    public string AllowedEngineType { get; set; } = "All";
    public string MaintenanceCategories { get; set; } = "TO-1,TO-2,Seasonal";
    public string CertificateEndorsementCode { get; set; } = string.Empty;
    public DateOnly ValidUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Engineer? Engineer { get; set; }
    public virtual UavModel? AllowedUavModel { get; set; }
}
