using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.DTOs.Insurance;

public record InsuranceCalculationRequestDto(
    decimal MtowKg,
    decimal? BasePriceRub = null,
    string? RegionCode = "FED",
    bool IncludeCasco = false
);

public record InsuranceCalculationResultDto(
    decimal MtowKg,
    decimal MrotRub,
    decimal CalculatedMinLiabilitySumRub, // MTOW * 2 * МРОТ
    decimal CivilLiabilityPremiumRub,
    decimal? CascoPremiumRub,
    decimal TotalPremiumRub,
    decimal AgentCommissionRub, // 30%
    decimal CommissionPercent
);

public record InsurancePolicyCreateDto(
    int UavInstanceId,
    int OwnerId,
    InsurancePolicyType PolicyType,
    decimal SumInsuredRub,
    decimal PremiumRub,
    DateOnly StartDate,
    DateOnly ExpiryDate,
    string? InsuranceCompanyName = null
);

public record InsurancePolicyDto(
    int PolicyId,
    string PolicyNumber,
    int UavInstanceId,
    string? UavSerialNumber,
    string? UavModelName,
    int OwnerId,
    string? OwnerName,
    InsurancePolicyType PolicyType,
    decimal CalculatedMinSumRub,
    decimal SumInsuredRub,
    decimal PremiumRub,
    decimal CommissionPercent,
    decimal CommissionRub,
    string InsuranceCompanyName,
    PolicyStatus Status,
    DateOnly StartDate,
    DateOnly ExpiryDate,
    DateTime CreatedAt
);
