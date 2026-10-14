namespace UavFleetManagement.Application.DTOs.Pilots;

// Пилоты / операторы БПЛА (маскирование лицензий и ВЛЭК)
public class UavUserPublicDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? MaskedLicenseNumber { get; set; }
    public bool IsActive { get; set; }
}

public class UavUserAdminDto : UavUserPublicDto
{
    public string? PilotLicenseNumber { get; set; }
    public DateOnly? LicenseIssueDate { get; set; }
    public DateOnly? LicenseExpiryDate { get; set; }
    public string? MedicalCertificateNumber { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class UavUserCreateDto
{
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
}

// Допуски пилотов к моделям
public class UserModelPermissionDto
{
    public int PermissionId { get; set; }
    public int UserId { get; set; }
    public string PilotFullName { get; set; } = string.Empty;
    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public DateOnly GrantedDate { get; set; }
    public string? InsurancePolicyNumber { get; set; }

    public UserModelPermissionDto() { }
    public UserModelPermissionDto(int permissionId, int userId, string pilotFullName, int modelId, string modelName, DateOnly grantedDate, string? insurancePolicyNumber)
    {
        PermissionId = permissionId;
        UserId = userId;
        PilotFullName = pilotFullName;
        ModelId = modelId;
        ModelName = modelName;
        GrantedDate = grantedDate;
        InsurancePolicyNumber = insurancePolicyNumber;
    }
}

public class UserModelPermissionCreateDto
{
    public int UserId { get; set; }
    public int ModelId { get; set; }
    public DateOnly? GrantedDate { get; set; }
    public string? InsurancePolicyNumber { get; set; }
}
