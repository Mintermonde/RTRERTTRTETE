using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Fleet;

public class UavOwner
{
    public int OwnerId { get; set; }
    public OwnerLegalType LegalType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Inn { get; set; }
    public string? Ogrn { get; set; }
    public string? Snils { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LegalAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UavInstance> Instances { get; set; } = new List<UavInstance>();
}
