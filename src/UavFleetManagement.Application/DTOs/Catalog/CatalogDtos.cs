using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.DTOs.Catalog;

// Производители
public class ManufacturerDto
{
    public int ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Inn { get; set; }
    public string? Website { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime CreatedAt { get; set; }

    public ManufacturerDto() { }
    public ManufacturerDto(int manufacturerId, string name, string? country, string? inn, string? website, string? contactPhone, DateTime createdAt)
    {
        ManufacturerId = manufacturerId;
        Name = name;
        Country = country;
        Inn = inn;
        Website = website;
        ContactPhone = contactPhone;
        CreatedAt = createdAt;
    }
}

public class ManufacturerCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; } = "Россия";
    public string? Inn { get; set; }
    public string? Website { get; set; }
    public string? ContactPhone { get; set; }
}

// Модели БПЛА (Разделение на Summary для User и Details для Superuser)
public class UavModelSummaryDto
{
    public int ModelId { get; set; }
    public int ManufacturerId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public string AerodynamicSchema { get; set; } = string.Empty;
    public string WeightCategory { get; set; } = string.Empty;
    public string ControlType { get; set; } = string.Empty;
    public decimal MtowKg { get; set; }
    public int? MaxFlightTimeMins { get; set; }
    public decimal? MaxRangeKm { get; set; }
}

public class UavModelDetailsDto : UavModelSummaryDto
{
    public decimal? BasePriceRub { get; set; } // Доступно только для Superuser
    public int? CruisingSpeedKmh { get; set; }
    public int? MaxAltitudeM { get; set; }
    public decimal? MaxWindSpeedMs { get; set; }
    public int MinTempCelsius { get; set; }
    public int MaxTempCelsius { get; set; }
    public int? LengthMm { get; set; }
    public int? WidthWingspanMm { get; set; }
    public int? HeightMm { get; set; }
}

public class UavModelCreateDto
{
    public int ManufacturerId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public UavAerodynamicSchema AerodynamicSchema { get; set; } = UavAerodynamicSchema.FixedWing;
    public UavWeightCategory WeightCategory { get; set; } = UavWeightCategory.Range150gTo30kg;
    public UavControlType ControlType { get; set; } = UavControlType.SemiAutonomous;
    public decimal MtowKg { get; set; }
    public int? MaxFlightTimeMins { get; set; }
    public decimal? MaxRangeKm { get; set; }
    public int? CruisingSpeedKmh { get; set; }
    public int? MaxAltitudeM { get; set; }
    public decimal? MaxWindSpeedMs { get; set; }
    public int MinTempCelsius { get; set; } = -20;
    public int MaxTempCelsius { get; set; } = 40;
    public int? LengthMm { get; set; }
    public int? WidthWingspanMm { get; set; }
    public int? HeightMm { get; set; }
    public decimal? BasePriceRub { get; set; }
}

// Справочник компонентов
public class ComponentCatalogDto
{
    public int ComponentTypeId { get; set; }
    public int ManufacturerId { get; set; }
    public string ManufacturerName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? MtbfHours { get; set; }
    public int? WeightG { get; set; }
    public string? SpecificationsJson { get; set; }

    public ComponentCatalogDto() { }
    public ComponentCatalogDto(int componentTypeId, int manufacturerId, string manufacturerName, string partNumber, string componentName, string category, int? mtbfHours, int? weightG, string? specificationsJson)
    {
        ComponentTypeId = componentTypeId;
        ManufacturerId = manufacturerId;
        ManufacturerName = manufacturerName;
        PartNumber = partNumber;
        ComponentName = componentName;
        Category = category;
        MtbfHours = mtbfHours;
        WeightG = weightG;
        SpecificationsJson = specificationsJson;
    }
}

public class ComponentCatalogCreateDto
{
    public int ManufacturerId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int? MtbfHours { get; set; }
    public int? WeightG { get; set; }
    public string? SpecificationsJson { get; set; }
}
