namespace UavFleetManagement.Application.DTOs.Engineers;

public class EngineerDto
{
    public int EngineerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string QualificationCategory { get; set; } = string.Empty;
    public string CertificationNumber { get; set; } = string.Empty;
    public DateOnly CertificationExpiryDate { get; set; }
    public string? Phone { get; set; } // Для обычных пользователей маскируется или скрывается
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

public class EngineerCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string QualificationCategory { get; set; } = string.Empty;
    public string CertificationNumber { get; set; } = string.Empty;
    public DateOnly CertificationExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}
