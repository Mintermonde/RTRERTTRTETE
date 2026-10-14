using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Entities.Maintenance;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Fleet;

public class UavInstance
{
    public int InstanceId { get; set; }
    public int ModelId { get; set; }
    public int? OwnerId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? RosaviatsiaNumber { get; set; }
    public DateOnly? ProductionDate { get; set; }
    public decimal TotalFlightTimeHours { get; set; } = 0.00m;
    public UavInstanceStatus CurrentStatus { get; set; } = UavInstanceStatus.Storage;
    public string? HullInsurancePolicy { get; set; }
    public DateOnly? InsuranceExpiryDate { get; set; }
    public string? PrimaryColor { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    public UavModel? Model { get; set; }
    public UavOwner? Owner { get; set; }
    public ICollection<InstanceComponent> InstalledComponents { get; set; } = new List<InstanceComponent>();
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
}
