using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Fleet;
using UavFleetManagement.Domain.Entities.Fleet;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UavOwnersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UavOwnersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        bool isSuperuser = User.IsInRole("Superuser");

        if (isSuperuser)
        {
            var adminList = await _context.UavOwners
                .Select(o => new UavOwnerAdminDto
                {
                    OwnerId = o.OwnerId,
                    LegalType = o.LegalType,
                    Name = o.Name,
                    Inn = o.Inn,
                    Ogrn = o.Ogrn,
                    Snils = o.Snils,
                    ContactEmail = o.ContactEmail,
                    ContactPhone = o.ContactPhone,
                    LegalAddress = o.LegalAddress,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return Ok(adminList);
        }

        // Обычные пользователи получают данные без ИНН, СНИЛС, ОГРН и точного адреса
        var publicList = await _context.UavOwners
            .Select(o => new UavOwnerPublicDto
            {
                OwnerId = o.OwnerId,
                LegalType = o.LegalType,
                Name = o.Name,
                ContactEmail = o.ContactEmail,
                ContactPhone = o.ContactPhone
            })
            .ToListAsync();

        return Ok(publicList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var o = await _context.UavOwners.FindAsync(id);
        if (o == null) throw new NotFoundException(nameof(UavOwner), id);

        bool isSuperuser = User.IsInRole("Superuser");
        if (isSuperuser)
        {
            return Ok(new UavOwnerAdminDto
            {
                OwnerId = o.OwnerId,
                LegalType = o.LegalType,
                Name = o.Name,
                Inn = o.Inn,
                Ogrn = o.Ogrn,
                Snils = o.Snils,
                ContactEmail = o.ContactEmail,
                ContactPhone = o.ContactPhone,
                LegalAddress = o.LegalAddress,
                CreatedAt = o.CreatedAt
            });
        }

        return Ok(new UavOwnerPublicDto
        {
            OwnerId = o.OwnerId,
            LegalType = o.LegalType,
            Name = o.Name,
            ContactEmail = o.ContactEmail,
            ContactPhone = o.ContactPhone
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] UavOwnerCreateDto dto)
    {
        var owner = new UavOwner
        {
            LegalType = dto.LegalType,
            Name = dto.Name,
            Inn = dto.Inn,
            Ogrn = dto.Ogrn,
            Snils = dto.Snils,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            LegalAddress = dto.LegalAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.UavOwners.Add(owner);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = owner.OwnerId }, owner);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] UavOwnerCreateDto dto)
    {
        var owner = await _context.UavOwners.FindAsync(id);
        if (owner == null) throw new NotFoundException(nameof(UavOwner), id);

        owner.LegalType = dto.LegalType;
        owner.Name = dto.Name;
        owner.Inn = dto.Inn;
        owner.Ogrn = dto.Ogrn;
        owner.Snils = dto.Snils;
        owner.ContactEmail = dto.ContactEmail;
        owner.ContactPhone = dto.ContactPhone;
        owner.LegalAddress = dto.LegalAddress;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var owner = await _context.UavOwners.FindAsync(id);
        if (owner == null) throw new NotFoundException(nameof(UavOwner), id);

        _context.UavOwners.Remove(owner);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
