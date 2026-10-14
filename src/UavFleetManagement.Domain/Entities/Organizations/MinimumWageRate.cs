namespace UavFleetManagement.Domain.Entities.Organizations;

public class MinimumWageRate
{
    public int RateId { get; set; }
    public string RegionCode { get; set; } = "FED";
    public string RegionName { get; set; } = "Российская Федерация";
    public decimal MrotValueRub { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
