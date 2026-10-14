using UavFleetManagement.Domain.Entities.Identity;
using UavFleetManagement.Domain.Entities.Maintenance;

namespace UavFleetManagement.Domain.Entities.Engineers;

public class Engineer
{
    public int EngineerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string QualificationCategory { get; set; } = string.Empty;
    public string CertificationNumber { get; set; } = string.Empty;
    public DateOnly CertificationExpiryDate { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
    public UserCredential? UserCredential { get; set; }
}
