using UavFleetManagement.Domain.Entities.Catalog;

namespace UavFleetManagement.Domain.Entities.Pilots;

public class UserModelPermission
{
    public int PermissionId { get; set; }
    public int UserId { get; set; }
    public int ModelId { get; set; }
    public DateOnly GrantedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? InsurancePolicyNumber { get; set; }

    public UavUser? User { get; set; }
    public UavModel? Model { get; set; }
}
