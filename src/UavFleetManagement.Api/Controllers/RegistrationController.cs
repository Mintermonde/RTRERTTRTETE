using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Registration;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Entities.Registration;
using UavFleetManagement.Domain.Enums;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegistrationController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public RegistrationController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Подача заявления на госучет БАС по ПП РФ № 658 (Асинхронный ответ 202 Accepted)
    /// </summary>
    [HttpPost("applications")]
    public async Task<IActionResult> CreateApplication([FromBody] RegistrationApplicationCreateDto dto)
    {
        var appNumber = $"REG-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}";
        int slaDays = dto.ApplicationType switch
        {
            UavApplicationType.InitialRegistration => 5,
            UavApplicationType.ModificationOfData => 5,
            UavApplicationType.Deregistration => 3,
            UavApplicationType.DuplicateOrExtract => 1,
            _ => 5
        };

        var app = new UavRegistrationApplication
        {
            ApplicationNumber = appNumber,
            ApplicationType = dto.ApplicationType,
            UavInstanceId = dto.UavInstanceId,
            ApplicantOwnerId = dto.ApplicantOwnerId,
            ApplicantType = dto.ApplicantType,
            UavModelName = dto.UavModelName,
            SerialNumber = dto.SerialNumber,
            EngineType = dto.EngineType,
            EngineCount = dto.EngineCount,
            MtowKg = dto.MtowKg,
            MaxTakeoffWeightCategory = dto.MaxTakeoffWeightCategory,
            SlaWorkingDays = slaDays,
            Status = UavApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            SlaDeadline = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(slaDays)),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UavRegistrationApplications.Add(app);

        // Transactional Outbox: атомарно в рамках одной транзакции сохраняем событие
        var outboxPayload = new
        {
            applicationId = 0, // будет обновлен после вычисления или в событии
            applicationNumber = appNumber,
            applicationType = dto.ApplicationType.ToString(),
            applicantOwnerId = dto.ApplicantOwnerId,
            uavModelName = dto.UavModelName,
            serialNumber = dto.SerialNumber,
            mtowKg = dto.MtowKg,
            engineType = dto.EngineType,
            engineCount = dto.EngineCount,
            slaWorkingDays = slaDays,
            submittedAt = DateTime.UtcNow
        };

        var metadata = new
        {
            traceId = HttpContext.TraceIdentifier,
            correlationId = Guid.NewGuid().ToString(),
            userId = User.Identity?.Name,
            role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value,
            timestamp = DateTime.UtcNow
        };

        // Сохраняем сущность, чтобы получить ID
        await _context.SaveChangesAsync();

        var outboxEvent = new OutboxEvent
        {
            AggregateType = "UavRegistration",
            AggregateId = app.ApplicationId.ToString(),
            EventType = "RegistrationApplicationCreatedEvent",
            Payload = JsonSerializer.Serialize(outboxPayload with { applicationId = app.ApplicationId }),
            Metadata = JsonSerializer.Serialize(metadata),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.OutboxEvents.Add(outboxEvent);
        await _context.SaveChangesAsync();

        // Немедленный ответ 202 Accepted
        return Accepted($"/api/registration/applications/{app.ApplicationId}", new
        {
            applicationId = app.ApplicationId,
            applicationNumber = app.ApplicationNumber,
            status = app.Status.ToString(),
            slaWorkingDays = app.SlaWorkingDays,
            slaDeadline = app.SlaDeadline,
            message = "Заявление принято в обработку (СМЭВ 4 / Росавиация)."
        });
    }

    /// <summary>
    /// Получение реестра заявлений
    /// </summary>
    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications()
    {
        var apps = await _context.UavRegistrationApplications
            .Include(a => a.ApplicantOwner)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new RegistrationApplicationDto(
                a.ApplicationId,
                a.ApplicationNumber,
                a.ApplicationType,
                a.UavInstanceId,
                a.ApplicantOwnerId,
                a.ApplicantOwner != null ? a.ApplicantOwner.Name : null,
                a.ApplicantType,
                a.UavModelName,
                a.SerialNumber,
                a.EngineType,
                a.EngineCount,
                a.MtowKg,
                a.MaxTakeoffWeightCategory,
                a.SlaWorkingDays,
                a.Status,
                a.RejectionReason,
                a.RosaviatsiaNumber,
                a.SubmittedAt,
                a.ProcessedAt,
                a.SlaDeadline
            ))
            .ToListAsync();

        return Ok(apps);
    }

    /// <summary>
    /// Получение конкретного заявления по ID
    /// </summary>
    [HttpGet("applications/{id}")]
    public async Task<IActionResult> GetApplicationById(int id)
    {
        var a = await _context.UavRegistrationApplications
            .Include(x => x.ApplicantOwner)
            .FirstOrDefaultAsync(x => x.ApplicationId == id && !x.IsDeleted);

        if (a == null) throw new NotFoundException(nameof(UavRegistrationApplication), id);

        return Ok(new RegistrationApplicationDto(
            a.ApplicationId,
            a.ApplicationNumber,
            a.ApplicationType,
            a.UavInstanceId,
            a.ApplicantOwnerId,
            a.ApplicantOwner?.Name,
            a.ApplicantType,
            a.UavModelName,
            a.SerialNumber,
            a.EngineType,
            a.EngineCount,
            a.MtowKg,
            a.MaxTakeoffWeightCategory,
            a.SlaWorkingDays,
            a.Status,
            a.RejectionReason,
            a.RosaviatsiaNumber,
            a.SubmittedAt,
            a.ProcessedAt,
            a.SlaDeadline
        ));
    }

    /// <summary>
    /// Обновление статуса заявления (Инспектор / Администратор)
    /// </summary>
    [HttpPatch("applications/{id}/status")]
    [Authorize(Roles = "Superuser,Auditor,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] RegistrationStatusUpdateDto dto)
    {
        var a = await _context.UavRegistrationApplications.FirstOrDefaultAsync(x => x.ApplicationId == id);
        if (a == null) throw new NotFoundException(nameof(UavRegistrationApplication), id);

        a.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.RosaviatsiaNumber)) a.RosaviatsiaNumber = dto.RosaviatsiaNumber;
        if (!string.IsNullOrEmpty(dto.RejectionReason)) a.RejectionReason = dto.RejectionReason;
        a.ProcessedAt = DateTime.UtcNow;
        a.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(a);
    }
}
