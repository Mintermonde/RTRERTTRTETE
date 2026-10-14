using System.Text.Json;
using FluentAssertions;
using UavFleetManagement.Application.Common.Events;
using Xunit;

namespace UavFleetManagement.UnitTests;

public class OutboxEventSerializationTests
{
    [Fact]
    public void RegistrationApplicationCreatedEvent_SerializationAndDeserialization_PreservesAllFields()
    {
        // Arrange
        var originalEvent = new RegistrationApplicationCreatedEvent(
            ApplicationId: 105,
            ApplicationNumber: "REG-2026-00105",
            ApplicationType: "InitialRegistration",
            ApplicantOwnerId: 12,
            UavModelName: "Геоскан 401",
            SerialNumber: "GS-401-2026-999",
            MtowKg: 9.30m,
            EngineType: "Electric",
            EngineCount: 4,
            SlaWorkingDays: 5
        );

        // Act
        var json = JsonSerializer.Serialize(originalEvent);
        var deserialized = JsonSerializer.Deserialize<RegistrationApplicationCreatedEvent>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ApplicationId.Should().Be(105);
        deserialized.ApplicationNumber.Should().Be("REG-2026-00105");
        deserialized.UavModelName.Should().Be("Геоскан 401");
        deserialized.MtowKg.Should().Be(9.30m);
        deserialized.AggregateType.Should().Be("UavRegistration");
        deserialized.AggregateId.Should().Be("105");
    }

    [Fact]
    public void PolicyIssuedEvent_PayloadContainsCommission30Percent()
    {
        // Arrange
        var policyEvent = new PolicyIssuedEvent(
            PolicyId: 42,
            PolicyNumber: "POLIS-VZK-2026-0042",
            UavInstanceId: 7,
            OwnerId: 3,
            PolicyType: "CivilLiabilityVzk131",
            SumInsuredRub: 500000.00m,
            PremiumRub: 10000.00m,
            CommissionRub: 3000.00m
        );

        // Act
        var json = JsonSerializer.Serialize(policyEvent);

        // Assert
        json.Should().Contain("\"CommissionRub\":3000.00");
        json.Should().Contain("\"PolicyNumber\":\"POLIS-VZK-2026-0042\"");
    }
}
