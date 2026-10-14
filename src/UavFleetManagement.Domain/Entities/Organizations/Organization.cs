using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Organizations;

public class Organization
{
    public int OrganizationId { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string Inn { get; set; } = string.Empty;
    public string? Kpp { get; set; }
    public string? Ogrn { get; set; }
    public OwnerLegalType LegalType { get; set; } = OwnerLegalType.LE;
    public string Industry { get; set; } = string.Empty;
    public string? LegalAddress { get; set; }
    public string? ActualAddress { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerificationDate { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
