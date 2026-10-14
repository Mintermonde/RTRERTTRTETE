using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Insurance;

public class InsurancePolicy
{
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public int UavInstanceId { get; set; }
    public int OwnerId { get; set; }
    public InsurancePolicyType PolicyType { get; set; } = InsurancePolicyType.CivilLiabilityVzk131;

    public decimal CalculatedMinSumRub { get; set; }
    public decimal SumInsuredRub { get; set; }
    public decimal PremiumRub { get; set; }
    public decimal CommissionPercent { get; set; } = 30.00m;
    public decimal CommissionRub { get; set; }

    public string InsuranceCompanyName { get; set; } = "АО \"АвиаСтрахование\"";
    public PolicyStatus Status { get; set; } = PolicyStatus.Draft;

    public DateOnly StartDate { get; set; }
    public DateOnly ExpiryDate { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual UavInstance? UavInstance { get; set; }
    public virtual UavOwner? Owner { get; set; }
}
