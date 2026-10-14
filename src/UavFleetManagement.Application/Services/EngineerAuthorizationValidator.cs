using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Application.Services;

public record AuthorizationValidationResult(bool IsAllowed, string? ErrorMessage = null);

public static class EngineerAuthorizationValidator
{
    public static AuthorizationValidationResult Validate(
        Engineer? engineer,
        IEnumerable<EngineerRepairAuthorization> authorizations,
        int modelId,
        string engineType,
        MaintenanceType maintenanceType,
        DateOnly currentDate)
    {
        if (engineer == null)
            return new(false, "Специалист ТОиР не найден.");

        if (!engineer.IsActive)
            return new(false, "Инженер заблокирован или неактивен.");

        if (engineer.CertificationExpiryDate < currentDate)
            return new(false, $"Срок действия сертификата инженера ({engineer.CertificationNumber}) истек {engineer.CertificationExpiryDate:dd.MM.yyyy}.");

        var activeAuths = authorizations
            .Where(a => a.IsActive && a.ValidUntil >= currentDate)
            .ToList();

        if (!activeAuths.Any())
            return new(false, "У инженера отсутствуют действующие вкладыши допуска к выполнению ТО.");

        string requiredCategory = maintenanceType switch
        {
            MaintenanceType.PreFlight => "PreFlight",
            MaintenanceType.PostFlight => "PostFlight",
            MaintenanceType.TO1 => "TO-1",
            MaintenanceType.TO2 => "TO-2",
            MaintenanceType.Seasonal => "Seasonal",
            _ => maintenanceType.ToString()
        };

        bool hasModelOrEngineMatch = activeAuths.Any(a =>
            (a.AllowedUavModelId == null || a.AllowedUavModelId == modelId) &&
            (a.AllowedEngineType == "All" || string.Equals(a.AllowedEngineType, engineType, StringComparison.OrdinalIgnoreCase)) &&
            (a.MaintenanceCategories.Contains(requiredCategory, StringComparison.OrdinalIgnoreCase) || a.MaintenanceCategories.Contains("All", StringComparison.OrdinalIgnoreCase))
        );

        if (!hasModelOrEngineMatch)
        {
            return new(false, $"Инженер не имеет подтвержденного допуска для модели #{modelId} (двигатель: {engineType}) по категории {requiredCategory}.");
        }

        return new(true);
    }
}
