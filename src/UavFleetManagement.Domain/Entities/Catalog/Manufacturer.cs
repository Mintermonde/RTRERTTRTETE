namespace UavFleetManagement.Domain.Entities.Catalog;

public class Manufacturer
{
    public int ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; } = "Россия";
    public string? Inn { get; set; }
    public string? Website { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UavModel> Models { get; set; } = new List<UavModel>();
    public ICollection<ComponentCatalog> Components { get; set; } = new List<ComponentCatalog>();
}
