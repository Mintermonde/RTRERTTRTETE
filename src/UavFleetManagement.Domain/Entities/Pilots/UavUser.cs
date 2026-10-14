using UavFleetManagement.Domain.Entities.Identity;

namespace UavFleetManagement.Domain.Entities.Pilots;

public class UavUser
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PilotLicenseNumber { get; set; }
    public DateOnly? LicenseIssueDate { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public string? MedicalCertificateNumber { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserModelPermission> Permissions { get; set; } = new List<UserModelPermission>();
    public UserCredential? UserCredential { get; set; }
}
