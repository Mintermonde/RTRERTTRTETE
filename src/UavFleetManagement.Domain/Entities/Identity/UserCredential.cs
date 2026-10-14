using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Entities.Pilots;

namespace UavFleetManagement.Domain.Entities.Identity;

public class UserCredential
{
    public int UserCredentialId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public int? LinkedUserId { get; set; }
    public UavUser? LinkedUser { get; set; }

    public int? LinkedEngineerId { get; set; }
    public Engineer? LinkedEngineer { get; set; }

    public bool IsBlocked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
