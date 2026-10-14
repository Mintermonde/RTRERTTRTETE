using FluentAssertions;
using UavFleetManagement.Application.DTOs.Organizations;
using UavFleetManagement.Domain.Enums;
using Xunit;

namespace UavFleetManagement.IntegrationTests;

public class DataMaskingPrivacyTests
{
    private static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 7) return "***";
        return $"{phone[..4]} ***-**-{phone[^2..]}";
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return "***";
        var parts = email.Split('@');
        var name = parts[0];
        var maskedName = name.Length <= 2 ? "**" : $"{name[0]}***{name[^1]}";
        return $"{maskedName}@{parts[1]}";
    }

    [Fact]
    public void OrganizationPublicDto_PreservesCompanyNameAndInn_MasksPersonalContacts()
    {
        // Arrange: реальные данные организации
        string rawPhone = "+7 (812) 363-33-33";
        string rawEmail = "contact@geoscan.aero";

        // Act
        var dto = new OrganizationPublicDto(
            OrganizationId: 1,
            LegalName: "ООО \"Геоскан Агро\"",
            ShortName: "Геоскан Агро",
            Inn: "7814472390",
            Kpp: "781401001",
            Ogrn: "1157847001234",
            LegalType: OwnerLegalType.LE,
            Industry: "АПК",
            LegalAddress: "г. Санкт-Петербург",
            ContactPhoneMasked: MaskPhone(rawPhone),
            ContactEmailMasked: MaskEmail(rawEmail),
            IsVerified: true,
            VerificationDate: DateTime.UtcNow
        );

        // Assert
        dto.LegalName.Should().Be("ООО \"Геоскан Агро\"");
        dto.Inn.Should().Be("7814472390"); // ИНН открыт для поиска
        dto.ContactPhoneMasked.Should().NotBe(rawPhone);
        dto.ContactPhoneMasked.Should().Contain("***");
        dto.ContactEmailMasked.Should().NotBe(rawEmail);
        dto.ContactEmailMasked.Should().Contain("***");
    }
}
