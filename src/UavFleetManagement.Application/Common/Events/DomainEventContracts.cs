namespace UavFleetManagement.Application.Common.Events;

public record RegistrationApplicationCreatedEvent(
    int ApplicationId,
    string ApplicationNumber,
    string ApplicationType,
    int ApplicantOwnerId,
    string UavModelName,
    string SerialNumber,
    decimal MtowKg,
    string EngineType,
    int EngineCount,
    int SlaWorkingDays
) : IntegrationEvent
{
    public override string AggregateType => "UavRegistration";
    public override string AggregateId => ApplicationId.ToString();
    public override string EventType => nameof(RegistrationApplicationCreatedEvent);
}

public record InnVerificationRequestedEvent(
    int OrganizationId,
    string Inn,
    string? Ogrn,
    string LegalName
) : IntegrationEvent
{
    public override string AggregateType => "PartyVerification";
    public override string AggregateId => OrganizationId.ToString();
    public override string EventType => nameof(InnVerificationRequestedEvent);
}

public record PolicyIssuedEvent(
    int PolicyId,
    string PolicyNumber,
    int UavInstanceId,
    int OwnerId,
    string PolicyType,
    decimal SumInsuredRub,
    decimal PremiumRub,
    decimal CommissionRub
) : IntegrationEvent
{
    public override string AggregateType => "InsurancePolicy";
    public override string AggregateId => PolicyId.ToString();
    public override string EventType => nameof(PolicyIssuedEvent);
}

public record FileAttachmentUploadedEvent(
    Guid FileId,
    string BucketName,
    string StorageKey,
    string OriginalFileName,
    long FileSizeBytes,
    string Sha256Checksum,
    string EntityType,
    string EntityId
) : IntegrationEvent
{
    public override string AggregateType => "Storage";
    public override string AggregateId => FileId.ToString();
    public override string EventType => nameof(FileAttachmentUploadedEvent);
}
