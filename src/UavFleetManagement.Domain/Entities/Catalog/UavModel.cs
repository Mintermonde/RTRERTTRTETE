using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Pilots;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Domain.Entities.Catalog;

public class UavModel
{
    public int ModelId { get; set; }
    public int ManufacturerId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public UavAerodynamicSchema AerodynamicSchema { get; set; }
    public UavWeightCategory WeightCategory { get; set; }
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

    public Manufacturer? Manufacturer { get; set; }
    public ICollection<UavInstance> Instances { get; set; } = new List<UavInstance>();
    public ICollection<UserModelPermission> Permissions { get; set; } = new List<UserModelPermission>();
}
