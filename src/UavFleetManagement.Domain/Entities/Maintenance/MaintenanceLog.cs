using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Maintenance;

public class MaintenanceLog
{
    public int LogId { get; set; }
    public int InstanceId { get; set; }
    public MaintenanceType TypeOfMaintenance { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public decimal UavOperatingHoursAtMaintenance { get; set; }
    public string? Description { get; set; }
    public string EngineerNameFallback { get; set; } = string.Empty;
    public int? EngineerId { get; set; }
    public DateOnly? NextMaintenanceDate { get; set; }

    public UavInstance? Instance { get; set; }
    public Engineer? Engineer { get; set; }
    public ICollection<MaintenanceComponentReplacement> Replacements { get; set; } = new List<MaintenanceComponentReplacement>();
}
