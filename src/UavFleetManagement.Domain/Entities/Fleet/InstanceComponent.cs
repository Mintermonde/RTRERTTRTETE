using UavFleetManagement.Domain.Entities.Catalog;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Fleet;

public class InstanceComponent
{
    public int InstalledComponentId { get; set; }
    public int InstanceId { get; set; }
    public int ComponentTypeId { get; set; }
    public string ComponentSerialNumber { get; set; } = string.Empty;
    public DateOnly InstallationDate { get; set; }
    public decimal ComponentOperatingHours { get; set; } = 0.00m;
    public bool IsActive { get; set; } = true;
    public ComponentCondition Condition { get; set; } = ComponentCondition.New;
    public int BatteryChargeCyclesInitial { get; set; } = 0;

    public UavInstance? Instance { get; set; }
    public ComponentCatalog? ComponentType { get; set; }
}
