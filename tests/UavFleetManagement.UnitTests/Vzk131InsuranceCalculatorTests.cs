using FluentAssertions;
using UavFleetManagement.Application.Services;
using Xunit;

namespace UavFleetManagement.UnitTests;

public class Vzk131InsuranceCalculatorTests
{
    [Fact]
    public void CalculateMinLiabilitySum_StandardParameters_ReturnsCorrectAmount()
    {
        // Arrange: Геоскан 401 (MTOW = 9.3 кг), МРОТ РФ = 19 242 руб.
        decimal mtowKg = 9.30m;
        decimal mrotRub = 19242.00m;
        decimal expected = 9.30m * 2m * 19242.00m; // 357,901.20 руб.

        // Act
        decimal result = VzkInsuranceCalculator.CalculateMinLiabilitySum(mtowKg, mrotRub);

        // Assert
        result.Should().Be(expected);
        result.Should().Be(357901.20m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CalculateMinLiabilitySum_InvalidMtow_ThrowsArgumentException(decimal invalidMtow)
    {
        // Act
        Action act = () => VzkInsuranceCalculator.CalculateMinLiabilitySum(invalidMtow, 19242.00m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Максимальная взлетная масса (MTOW) должна быть строго положительной*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void CalculateMinLiabilitySum_InvalidMrot_ThrowsArgumentException(decimal invalidMrot)
    {
        // Act
        Action act = () => VzkInsuranceCalculator.CalculateMinLiabilitySum(10m, invalidMrot);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Значение МРОТ должно быть строго положительным*");
    }

    [Fact]
    public void CalculateAgentCommission_DefaultRate30Percent_ReturnsExactCommission()
    {
        // Arrange
        decimal premium = 10000.00m;
        decimal expectedCommission = 3000.00m; // 30%

        // Act
        decimal commission = VzkInsuranceCalculator.CalculateAgentCommission(premium, 30.0m);

        // Assert
        commission.Should().Be(expectedCommission);
    }

    [Fact]
    public void CalculateCascoPremium_ValidBasePrice_CalculatesExpectedPercentage()
    {
        // Arrange: Стоимость БПЛА = 2 000 000 руб., ставка 4.5%
        decimal basePrice = 2000000.00m;
        decimal expectedCasco = 90000.00m;

        // Act
        decimal casco = VzkInsuranceCalculator.CalculateCascoPremium(basePrice, 4.5m);

        // Assert
        casco.Should().Be(expectedCasco);
    }
}
