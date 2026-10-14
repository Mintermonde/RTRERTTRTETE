using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.DTOs.Maintenance;

public class MaintenanceLogDto
{
    public int LogId { get; set; }
    public int InstanceId { get; set; }
    public string InstanceSerialNumber { get; set; } = string.Empty;
    public MaintenanceType TypeOfMaintenance { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public decimal UavOperatingHoursAtMaintenance { get; set; }
    public string? Description { get; set; }
    public int? EngineerId { get; set; }
    public string? EngineerFullName { get; set; }
    public string? QualificationCategory { get; set; }
    public DateOnly? NextMaintenanceDate { get; set; }

    public MaintenanceLogDto() { }
    public MaintenanceLogDto(int logId, int instanceId, string instanceSerialNumber, MaintenanceType typeOfMaintenance, DateTime maintenanceDate, decimal uavOperatingHoursAtMaintenance, string? description, int? engineerId, string? engineerFullName, string? qualificationCategory, DateOnly? nextMaintenanceDate)
    {
        LogId = logId;
        InstanceId = instanceId;
        InstanceSerialNumber = instanceSerialNumber;
        TypeOfMaintenance = typeOfMaintenance;
        MaintenanceDate = maintenanceDate;
        UavOperatingHoursAtMaintenance = uavOperatingHoursAtMaintenance;
        Description = description;
        EngineerId = engineerId;
        EngineerFullName = engineerFullName;
        QualificationCategory = qualificationCategory;
        NextMaintenanceDate = nextMaintenanceDate;
    }
}

public class MaintenanceLogCreateDto
{
    public int InstanceId { get; set; }
    public MaintenanceType TypeOfMaintenance { get; set; } = MaintenanceType.PreFlight;
    public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;
    public decimal UavOperatingHoursAtMaintenance { get; set; } = 0.00m;
    public string? Description { get; set; }
    public int? EngineerId { get; set; }
    public string? EngineerNameFallback { get; set; }
    public DateOnly? NextMaintenanceDate { get; set; }
}

public class MaintenanceReplacementDto
{
    public int ReplacementId { get; set; }
    public int LogId { get; set; }
    public int? RemovedComponentId { get; set; }
    public string? RemovedSerialNumber { get; set; }
    public int? InstalledComponentId { get; set; }
    public string? InstalledSerialNumber { get; set; }
    public string? ReasonForReplacement { get; set; }

    public MaintenanceReplacementDto() { }
    public MaintenanceReplacementDto(int replacementId, int logId, int? removedComponentId, string? removedSerialNumber, int? installedComponentId, string? installedSerialNumber, string? reasonForReplacement)
    {
        ReplacementId = replacementId;
        LogId = logId;
        RemovedComponentId = removedComponentId;
        RemovedSerialNumber = removedSerialNumber;
        InstalledComponentId = installedComponentId;
        InstalledSerialNumber = installedSerialNumber;
        ReasonForReplacement = reasonForReplacement;
    }
}

public class MaintenanceReplacementCreateDto
{
    public int LogId { get; set; }
    public int? RemovedComponentId { get; set; }
    public int? InstalledComponentId { get; set; }
    public string? ReasonForReplacement { get; set; }
}
