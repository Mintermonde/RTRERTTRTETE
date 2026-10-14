using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.DTOs.Registration;

public record RegistrationApplicationCreateDto(
    UavApplicationType ApplicationType,
    int? UavInstanceId,
    int ApplicantOwnerId,
    OwnerLegalType ApplicantType,
    string UavModelName,
    string SerialNumber,
    string EngineType,
    int EngineCount,
    decimal MtowKg,
    UavWeightCategory MaxTakeoffWeightCategory,
    List<Guid>? AttachmentFileIds = null
);

public record RegistrationApplicationDto(
    int ApplicationId,
    string ApplicationNumber,
    UavApplicationType ApplicationType,
    int? UavInstanceId,
    int ApplicantOwnerId,
    string? ApplicantOwnerName,
    OwnerLegalType ApplicantType,
    string UavModelName,
    string SerialNumber,
    string EngineType,
    int EngineCount,
    decimal MtowKg,
    UavWeightCategory MaxTakeoffWeightCategory,
    int SlaWorkingDays,
    UavApplicationStatus Status,
    string? RejectionReason,
    string? RosaviatsiaNumber,
    DateTime SubmittedAt,
    DateTime? ProcessedAt,
    DateOnly? SlaDeadline
);

public record RegistrationStatusUpdateDto(
    UavApplicationStatus Status,
    string? RosaviatsiaNumber,
    string? RejectionReason
);
