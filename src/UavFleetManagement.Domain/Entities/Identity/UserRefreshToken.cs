using UavFleetManagement.Domain.Entities.Identity;

namespace UavFleetManagement.Domain.Entities.Identity;

public class UserRefreshToken
{
    public int RefreshTokenId { get; set; }
    public int? UserId { get; set; }
    public int? SuperuserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    // Navigation
    public virtual UserCredential? User { get; set; }
    public virtual SuperuserCredential? Superuser { get; set; }
}
