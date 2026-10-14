using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Organizations;
using UavFleetManagement.Domain.Entities.Organizations;
using UavFleetManagement.Domain.Entities.Outbox;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public OrganizationsController(IApplicationDbContext context)
    {
        _context = context;
    }

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

    /// <summary>
    /// Реестр организаций и эксплуатантов (Открытый поиск с маскированием ПДн по 152-ФЗ)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrganizations([FromQuery] string? industry = null, [FromQuery] string? search = null)
    {
        bool isSuperuser = User.IsInRole("Superuser");

        var query = _context.Organizations.Where(o => !o.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(industry))
        {
            query = query.Where(o => o.Industry.ToLower().Contains(industry.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.LegalName.ToLower().Contains(s) || o.Inn.Contains(s));
        }

        var list = await query.OrderBy(o => o.LegalName).ToListAsync();

        var result = list.Select(o => new OrganizationPublicDto(
            o.OrganizationId,
            o.LegalName,
            o.ShortName,
            o.Inn,
            o.Kpp,
            o.Ogrn,
            o.LegalType,
            o.Industry,
            o.LegalAddress,
            isSuperuser ? o.ContactPhone : MaskPhone(o.ContactPhone),
            isSuperuser ? o.ContactEmail : MaskEmail(o.ContactEmail),
            o.IsVerified,
            o.VerificationDate
        ));

        return Ok(result);
    }

    /// <summary>
    /// Получение карточки организации по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var o = await _context.Organizations.FirstOrDefaultAsync(x => x.OrganizationId == id && !x.IsDeleted);
        if (o == null) throw new NotFoundException(nameof(Organization), id);

        bool isSuperuser = User.IsInRole("Superuser");

        if (isSuperuser)
        {
            return Ok(new OrganizationAdminDto(
                o.OrganizationId,
                o.LegalName,
                o.ShortName,
                o.Inn,
                o.Kpp,
                o.Ogrn,
                o.LegalType,
                o.Industry,
                o.LegalAddress,
                o.ActualAddress,
                o.ContactPhone,
                o.ContactEmail,
                o.IsVerified,
                o.VerificationDate,
                o.CreatedAt
            ));
        }

        return Ok(new OrganizationPublicDto(
            o.OrganizationId,
            o.LegalName,
            o.ShortName,
            o.Inn,
            o.Kpp,
            o.Ogrn,
            o.LegalType,
            o.Industry,
            o.LegalAddress,
            MaskPhone(o.ContactPhone),
            MaskEmail(o.ContactEmail),
            o.IsVerified,
            o.VerificationDate
        ));
    }

    /// <summary>
    /// Регистрация организации (инициирует проверку ИНН/ОГРН через СМЭВ 3 / ФНС)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] OrganizationCreateDto dto)
    {
        if (await _context.Organizations.AnyAsync(o => o.Inn == dto.Inn && !o.IsDeleted))
        {
            throw new BadRequestException($"Организация с ИНН {dto.Inn} уже зарегистрирована в системе.");
        }

        var org = new Organization
        {
            LegalName = dto.LegalName,
            ShortName = dto.ShortName,
            Inn = dto.Inn,
            Kpp = dto.Kpp,
            Ogrn = dto.Ogrn,
            LegalType = dto.LegalType,
            Industry = dto.Industry,
            LegalAddress = dto.LegalAddress,
            ActualAddress = dto.ActualAddress,
            ContactPhone = dto.ContactPhone,
            ContactEmail = dto.ContactEmail,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        // Transactional Outbox: запрос на верификацию в ФНС через СМЭВ 3
        var outboxEvent = new OutboxEvent
        {
            AggregateType = "PartyVerification",
            AggregateId = org.OrganizationId.ToString(),
            EventType = "InnVerificationRequestedEvent",
            Payload = JsonSerializer.Serialize(new
            {
                organizationId = org.OrganizationId,
                inn = org.Inn,
                ogrn = org.Ogrn,
                legalName = org.LegalName
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

        return CreatedAtAction(nameof(GetById), new { id = org.OrganizationId }, org);
    }
}
