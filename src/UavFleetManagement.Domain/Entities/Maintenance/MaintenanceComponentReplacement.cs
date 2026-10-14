using UavFleetManagement.Domain.Entities.Fleet;

namespace UavFleetManagement.Domain.Entities.Maintenance;

public class MaintenanceComponentReplacement
{
    public int ReplacementId { get; set; }
    public int LogId { get; set; }
    public int? RemovedComponentId { get; set; }
    public int? InstalledComponentId { get; set; }
    public string? ReasonForReplacement { get; set; }

    public MaintenanceLog? MaintenanceLog { get; set; }
    public InstanceComponent? RemovedComponent { get; set; }
    public InstanceComponent? InstalledComponent { get; set; }
}
