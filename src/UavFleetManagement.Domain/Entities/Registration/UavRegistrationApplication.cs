using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Registration;

public class UavRegistrationApplication
{
    public int ApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public UavApplicationType ApplicationType { get; set; } = UavApplicationType.InitialRegistration;
    public int? UavInstanceId { get; set; }
    public int ApplicantOwnerId { get; set; }
    public OwnerLegalType ApplicantType { get; set; } = OwnerLegalType.LE;

    public string UavModelName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string EngineType { get; set; } = "Electric";
    public int EngineCount { get; set; } = 1;
    public decimal MtowKg { get; set; }
    public UavWeightCategory MaxTakeoffWeightCategory { get; set; } = UavWeightCategory.Range150gTo30kg;

    public int SlaWorkingDays { get; set; } = 5;
    public UavApplicationStatus Status { get; set; } = UavApplicationStatus.Pending;
    public string? RejectionReason { get; set; }
    public string? RosaviatsiaNumber { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateOnly? SlaDeadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigation
    public virtual UavOwner? ApplicantOwner { get; set; }
    public virtual UavInstance? UavInstance { get; set; }
}
