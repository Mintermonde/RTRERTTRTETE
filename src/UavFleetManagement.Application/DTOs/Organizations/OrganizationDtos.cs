using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.DTOs.Organizations;

public record OrganizationCreateDto(
    string LegalName,
    string? ShortName,
    string Inn,
    string? Kpp,
    string? Ogrn,
    OwnerLegalType LegalType,
    string Industry,
    string? LegalAddress,
    string? ActualAddress,
    string? ContactPhone,
    string? ContactEmail
);

public record OrganizationPublicDto(
    int OrganizationId,
    string LegalName,
    string? ShortName,
    string Inn,
    string? Kpp,
    string? Ogrn,
    OwnerLegalType LegalType,
    string Industry,
    string? LegalAddress,
    string? ContactPhoneMasked, // Маскировано по 152-ФЗ для обычных пользователей
    string? ContactEmailMasked,
    bool IsVerified,
    DateTime? VerificationDate
);

public record OrganizationAdminDto(
    int OrganizationId,
    string LegalName,
    string? ShortName,
    string Inn,
    string? Kpp,
    string? Ogrn,
    OwnerLegalType LegalType,
    string Industry,
    string? LegalAddress,
    string? ActualAddress,
    string? ContactPhone,
    string? ContactEmail,
    bool IsVerified,
    DateTime? VerificationDate,
    DateTime CreatedAt
);
