namespace UavFleetManagement.Application.Services;

public static class VzkInsuranceCalculator
{
    public const decimal DefaultCommissionRate = 0.30m; // 30% агентская комиссия

    /// <summary>
    /// Расчет минимальной страховой ответственности по ст. 131 ВЗК РФ:
    /// Лимит = MTOW (кг) * 2 * МРОТ (руб)
    /// </summary>
    public static decimal CalculateMinLiabilitySum(decimal mtowKg, decimal mrotRub)
    {
        if (mtowKg <= 0)
            throw new ArgumentException("Максимальная взлетная масса (MTOW) должна быть строго положительной.", nameof(mtowKg));

        if (mrotRub <= 0)
            throw new ArgumentException("Значение МРОТ должно быть строго положительным.", nameof(mrotRub));

        return Math.Round(mtowKg * 2m * mrotRub, 2);
    }

    /// <summary>
    /// Расчет страховой премии гражданской ответственности
    /// Базовая ставка 1.5% от страховой суммы
    /// </summary>
    public static decimal CalculateCivilLiabilityPremium(decimal sumInsuredRub, decimal baseRatePercent = 1.5m)
    {
        if (sumInsuredRub <= 0)
            throw new ArgumentException("Страховая сумма должна быть положительной.", nameof(sumInsuredRub));

        return Math.Round(sumInsuredRub * (baseRatePercent / 100m), 2);
    }

    /// <summary>
    /// Расчет премии КАСКО (от балансовой стоимости борта)
    /// </summary>
    public static decimal CalculateCascoPremium(decimal basePriceRub, decimal cascoRatePercent = 4.5m)
    {
        if (basePriceRub <= 0)
            throw new ArgumentException("Базовая стоимость борта должна быть положительной.", nameof(basePriceRub));

        return Math.Round(basePriceRub * (cascoRatePercent / 100m), 2);
    }

    /// <summary>
    /// Расчет агентской комиссии сервиса (30%)
    /// </summary>
    public static decimal CalculateAgentCommission(decimal premiumRub, decimal commissionPercent = 30.0m)
    {
        if (premiumRub < 0)
            throw new ArgumentException("Страховая премия не может быть отрицательной.", nameof(premiumRub));

        return Math.Round(premiumRub * (commissionPercent / 100m), 2);
    }
}
