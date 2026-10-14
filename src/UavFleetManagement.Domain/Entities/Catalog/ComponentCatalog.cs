using UavFleetManagement.Domain.Entities.Fleet;

namespace UavFleetManagement.Domain.Entities.Catalog;

public class ComponentCatalog
{
    public int ComponentTypeId { get; set; }
    public int ManufacturerId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? MtbfHours { get; set; }
    public int? WeightG { get; set; }
    public string? SpecificationsJson { get; set; } // JSONB

    public Manufacturer? Manufacturer { get; set; }
    public ICollection<InstanceComponent> InstanceComponents { get; set; } = new List<InstanceComponent>();
}
