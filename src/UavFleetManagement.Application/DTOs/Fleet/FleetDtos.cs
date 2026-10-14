using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.DTOs.Fleet;

// Владельцы БПЛА (маскирование ПДн)
public class UavOwnerPublicDto
{
    public int OwnerId { get; set; }
    public OwnerLegalType LegalType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}

public class UavOwnerAdminDto : UavOwnerPublicDto
{
    public string? Inn { get; set; }
    public string? Ogrn { get; set; }
    public string? Snils { get; set; }
    public string? LegalAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UavOwnerCreateDto
{
    public OwnerLegalType LegalType { get; set; } = OwnerLegalType.Individual;
    public string Name { get; set; } = string.Empty;
    public string? Inn { get; set; }
    public string? Ogrn { get; set; }
    public string? Snils { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LegalAddress { get; set; }
}

// Физические борта БПЛА
public class UavInstanceDto
{
    public int InstanceId { get; set; }
    public int ModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? RosaviatsiaNumber { get; set; }
    public DateOnly? ProductionDate { get; set; }
    public decimal TotalFlightTimeHours { get; set; }
    public UavInstanceStatus CurrentStatus { get; set; }
    public string? PrimaryColor { get; set; }
    public DateTime LastUpdate { get; set; }

    // Конфиденциальные данные: открыты только для Superuser (для обычных пользователей - null)
    public string? HullInsurancePolicy { get; set; }
    public DateOnly? InsuranceExpiryDate { get; set; }
}

public class UavInstanceCreateDto
{
    public int ModelId { get; set; }
    public int? OwnerId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? RosaviatsiaNumber { get; set; }
    public DateOnly? ProductionDate { get; set; }
    public decimal TotalFlightTimeHours { get; set; } = 0.00m;
    public UavInstanceStatus CurrentStatus { get; set; } = UavInstanceStatus.Storage;
    public string? HullInsurancePolicy { get; set; }
    public DateOnly? InsuranceExpiryDate { get; set; }
    public string? PrimaryColor { get; set; }
}

public class UavStatusUpdateDto
{
    public UavInstanceStatus Status { get; set; } = UavInstanceStatus.Active;
}

// Установленные компоненты
public class InstanceComponentDto
{
    public int InstalledComponentId { get; set; }
    public int InstanceId { get; set; }
    public int ComponentTypeId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentSerialNumber { get; set; } = string.Empty;
    public DateOnly InstallationDate { get; set; }
    public decimal ComponentOperatingHours { get; set; }
    public bool IsActive { get; set; }
    public ComponentCondition Condition { get; set; }
    public int BatteryChargeCyclesInitial { get; set; }

    public InstanceComponentDto() { }
    public InstanceComponentDto(int installedComponentId, int instanceId, int componentTypeId, string componentName, string componentSerialNumber, DateOnly installationDate, decimal componentOperatingHours, bool isActive, ComponentCondition condition, int batteryChargeCyclesInitial)
    {
        InstalledComponentId = installedComponentId;
        InstanceId = instanceId;
        ComponentTypeId = componentTypeId;
        ComponentName = componentName;
        ComponentSerialNumber = componentSerialNumber;
        InstallationDate = installationDate;
        ComponentOperatingHours = componentOperatingHours;
        IsActive = isActive;
        Condition = condition;
        BatteryChargeCyclesInitial = batteryChargeCyclesInitial;
    }
}

public class InstanceComponentInstallDto
{
    public int InstanceId { get; set; }
    public int ComponentTypeId { get; set; }
    public string ComponentSerialNumber { get; set; } = string.Empty;
    public DateOnly InstallationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal ComponentOperatingHours { get; set; } = 0.00m;
    public ComponentCondition Condition { get; set; } = ComponentCondition.New;
    public int BatteryChargeCyclesInitial { get; set; } = 0;
}
