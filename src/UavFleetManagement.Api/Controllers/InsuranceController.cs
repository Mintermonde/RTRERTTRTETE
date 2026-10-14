using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Insurance;
using UavFleetManagement.Application.Services;
using UavFleetManagement.Domain.Entities.Insurance;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InsuranceController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public InsuranceController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Калькулятор гражданской ответственности по ст. 131 ВЗК РФ и КАСКО
    /// </summary>
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateInsurance([FromBody] InsuranceCalculationRequestDto dto)
    {
        var regionCode = dto.RegionCode ?? "FED";
        var mrotRate = await _context.MinimumWageRates
            .Where(r => r.RegionCode == regionCode)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync();

        decimal mrot = mrotRate?.MrotValueRub ?? 19242.00m;

        decimal minLiabilitySum = VzkInsuranceCalculator.CalculateMinLiabilitySum(dto.MtowKg, mrot);
        decimal civilPremium = VzkInsuranceCalculator.CalculateCivilLiabilityPremium(minLiabilitySum);

        decimal? cascoPremium = null;
        if (dto.IncludeCasco && dto.BasePriceRub.HasValue && dto.BasePriceRub > 0)
        {
            cascoPremium = VzkInsuranceCalculator.CalculateCascoPremium(dto.BasePriceRub.Value);
        }

        decimal totalPremium = civilPremium + (cascoPremium ?? 0);
        decimal agentCommission = VzkInsuranceCalculator.CalculateAgentCommission(totalPremium, 30.0m);

        return Ok(new InsuranceCalculationResultDto(
            MtowKg: dto.MtowKg,
            MrotRub: mrot,
            CalculatedMinLiabilitySumRub: minLiabilitySum,
            CivilLiabilityPremiumRub: civilPremium,
            CascoPremiumRub: cascoPremium,
            TotalPremiumRub: totalPremium,
            AgentCommissionRub: agentCommission,
            CommissionPercent: 30.0m
        ));
    }

    /// <summary>
    /// Оформление страхового полиса (Транзакционный Outbox)
    /// </summary>
    [HttpPost("policies")]
    public async Task<IActionResult> CreatePolicy([FromBody] InsurancePolicyCreateDto dto)
    {
        var instance = await _context.UavInstances.Include(i => i.Model).FirstOrDefaultAsync(i => i.InstanceId == dto.UavInstanceId);
        if (instance == null) throw new NotFoundException("UavInstance", dto.UavInstanceId);

        decimal mtow = instance.Model?.MtowKg ?? 1.0m;
        decimal mrot = 19242.00m;
        decimal minSum = VzkInsuranceCalculator.CalculateMinLiabilitySum(mtow, mrot);

        var policyNumber = $"POLIS-VZK-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}";
        decimal commission = VzkInsuranceCalculator.CalculateAgentCommission(dto.PremiumRub, 30.0m);

        var policy = new InsurancePolicy
        {
            PolicyNumber = policyNumber,
            UavInstanceId = dto.UavInstanceId,
            OwnerId = dto.OwnerId,
            PolicyType = dto.PolicyType,
            CalculatedMinSumRub = minSum,
            SumInsuredRub = Math.Max(dto.SumInsuredRub, minSum),
            PremiumRub = dto.PremiumRub,
            CommissionPercent = 30.00m,
            CommissionRub = commission,
            InsuranceCompanyName = dto.InsuranceCompanyName ?? "АО \"АвиаСтрахование\"",
            Status = PolicyStatus.Pending,
            StartDate = dto.StartDate,
            ExpiryDate = dto.ExpiryDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.InsurancePolicies.Add(policy);
        await _context.SaveChangesAsync();

        // Transactional Outbox
        var outboxEvent = new OutboxEvent
        {
            AggregateType = "InsurancePolicy",
            AggregateId = policy.PolicyId.ToString(),
            EventType = "PolicyIssuedEvent",
            Payload = JsonSerializer.Serialize(new
            {
                policyId = policy.PolicyId,
                policyNumber = policy.PolicyNumber,
                uavInstanceId = policy.UavInstanceId,
                ownerId = policy.OwnerId,
                policyType = policy.PolicyType.ToString(),
                sumInsuredRub = policy.SumInsuredRub,
                premiumRub = policy.PremiumRub,
                commissionRub = policy.CommissionRub
            }),
            Metadata = JsonSerializer.Serialize(new
            {
                traceId = HttpContext.TraceIdentifier,
                userId = User.Identity?.Name,
                timestamp = DateTime.UtcNow
            }),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPolicies), new { id = policy.PolicyId }, policy);
    }

    /// <summary>
    /// Получение реестра страховых полисов
    /// </summary>
    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies()
    {
        var policies = await _context.InsurancePolicies
            .Include(p => p.UavInstance!).ThenInclude(i => i.Model)
            .Include(p => p.Owner)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new InsurancePolicyDto(
                p.PolicyId,
                p.PolicyNumber,
                p.UavInstanceId,
                p.UavInstance != null ? p.UavInstance.SerialNumber : null,
                p.UavInstance != null && p.UavInstance.Model != null ? p.UavInstance.Model.ModelName : null,
                p.OwnerId,
                p.Owner != null ? p.Owner.Name : null,
                p.PolicyType,
                p.CalculatedMinSumRub,
                p.SumInsuredRub,
                p.PremiumRub,
                p.CommissionPercent,
                p.CommissionRub,
                p.InsuranceCompanyName,
                p.Status,
                p.StartDate,
                p.ExpiryDate,
                p.CreatedAt
            ))
            .ToListAsync();

        return Ok(policies);
    }

    /// <summary>
    /// Экспорт реестра полисов в формате CSV
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Superuser,Agent,Accountant,Admin")]
    public async Task<IActionResult> ExportPoliciesCsv()
    {
        var policies = await _context.InsurancePolicies
            .Include(p => p.UavInstance!).ThenInclude(i => i.Model)
            .Include(p => p.Owner)
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID;Номер полиса;Серийный номер борта;Владелец;Тип полиса;Страховая сумма (руб);Премия (руб);Комиссия 30% (руб);Статус;Дата начала;Дата окончания");

        foreach (var p in policies)
        {
            sb.AppendLine($"{p.PolicyId};{p.PolicyNumber};{p.UavInstance?.SerialNumber};{p.Owner?.Name};{p.PolicyType};{p.SumInsuredRub};{p.PremiumRub};{p.CommissionRub};{p.Status};{p.StartDate:yyyy-MM-dd};{p.ExpiryDate:yyyy-MM-dd}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"insurance_registry_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
