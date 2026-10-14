using FluentAssertions;
using UavFleetManagement.Application.Services;
using UavFleetManagement.Domain.Entities.Engineers;
using UavFleetManagement.Domain.Enums;
using Xunit;

namespace UavFleetManagement.UnitTests;

public class EngineerAuthorizationValidatorTests
{
    private readonly Engineer _validEngineer = new()
    {
        EngineerId = 1,
        FirstName = "Иван",
        LastName = "Петров",
        CertificationNumber = "CERT-2026-001",
        CertificationExpiryDate = new DateOnly(2027, 12, 31),
        IsActive = true
    };

    [Fact]
    public void Validate_ActiveAuthorizedModelAndTO1_ReturnsAllowed()
    {
        // Arrange
        var auths = new List<EngineerRepairAuthorization>
        {
            new()
            {
                AuthId = 10,
                EngineerId = 1,
                AllowedUavModelId = 5,
                AllowedEngineType = "Electric",
                MaintenanceCategories = "TO-1,TO-2,Seasonal",
                CertificateEndorsementCode = "END-01",
                ValidUntil = new DateOnly(2027, 6, 1),
                IsActive = true
            }
        };

        var today = new DateOnly(2026, 10, 14);

        // Act
        var result = EngineerAuthorizationValidator.Validate(
            _validEngineer,
            auths,
            modelId: 5,
            engineType: "Electric",
            maintenanceType: MaintenanceType.TO1,
            currentDate: today
        );

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Validate_ExpiredCertificate_ReturnsNotAllowed()
    {
        // Arrange: Срок сертификата истек вчера
        var expiredEngineer = new Engineer
        {
            EngineerId = 2,
            FirstName = "Сергей",
            LastName = "Иванов",
            CertificationNumber = "CERT-OLD-999",
            CertificationExpiryDate = new DateOnly(2026, 10, 13),
            IsActive = true
        };

        var auths = new List<EngineerRepairAuthorization>
        {
            new() { AllowedUavModelId = 5, AllowedEngineType = "Electric", MaintenanceCategories = "TO-1", ValidUntil = new DateOnly(2027, 1, 1), IsActive = true }
        };

        var today = new DateOnly(2026, 10, 14);

        // Act
        var result = EngineerAuthorizationValidator.Validate(
            expiredEngineer,
            auths,
            modelId: 5,
            engineType: "Electric",
            maintenanceType: MaintenanceType.TO1,
            currentDate: today
        );

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Срок действия сертификата инженера (CERT-OLD-999) истек");
    }

    [Fact]
    public void Validate_ModelNotAuthorized_ReturnsNotAllowed()
    {
        // Arrange: Допуск только к модели 5, а проверяется модель 99
        var auths = new List<EngineerRepairAuthorization>
        {
            new()
            {
                AllowedUavModelId = 5,
                AllowedEngineType = "Electric",
                MaintenanceCategories = "TO-1,TO-2",
                ValidUntil = new DateOnly(2027, 1, 1),
                IsActive = true
            }
        };

        var today = new DateOnly(2026, 10, 14);

        // Act
        var result = EngineerAuthorizationValidator.Validate(
            _validEngineer,
            auths,
            modelId: 99,
            engineType: "Electric",
            maintenanceType: MaintenanceType.TO1,
            currentDate: today
        );

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Инженер не имеет подтвержденного допуска для модели #99");
    }
}
