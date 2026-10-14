using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Engineers;
using UavFleetManagement.Domain.Entities.Engineers;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EngineersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public EngineersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        bool isSuperuser = User.IsInRole("Superuser");

        var list = await _context.Engineers
            .Where(e => e.IsActive)
            .Select(e => new EngineerDto
            {
                EngineerId = e.EngineerId,
                FullName = $"{e.LastName} {e.FirstName} {e.MiddleName}".Trim(),
                FirstName = e.FirstName,
                LastName = e.LastName,
                MiddleName = e.MiddleName,
                QualificationCategory = e.QualificationCategory,
                CertificationNumber = e.CertificationNumber,
                CertificationExpiryDate = e.CertificationExpiryDate,
                Phone = isSuperuser ? e.Phone : null, // Личный телефон скрыт от обычных пользователей
                Email = e.Email,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _context.Engineers.FindAsync(id);
        if (e == null) throw new NotFoundException(nameof(Engineer), id);

        bool isSuperuser = User.IsInRole("Superuser");
        return Ok(new EngineerDto
        {
            EngineerId = e.EngineerId,
            FullName = $"{e.LastName} {e.FirstName} {e.MiddleName}".Trim(),
            FirstName = e.FirstName,
            LastName = e.LastName,
            MiddleName = e.MiddleName,
            QualificationCategory = e.QualificationCategory,
            CertificationNumber = e.CertificationNumber,
            CertificationExpiryDate = e.CertificationExpiryDate,
            Phone = isSuperuser ? e.Phone : null,
            Email = e.Email,
            IsActive = e.IsActive
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] EngineerCreateDto dto)
    {
        var engineer = new Engineer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            QualificationCategory = dto.QualificationCategory,
            CertificationNumber = dto.CertificationNumber,
            CertificationExpiryDate = dto.CertificationExpiryDate,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = dto.IsActive
        };

        _context.Engineers.Add(engineer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = engineer.EngineerId }, engineer);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] EngineerCreateDto dto)
    {
        var engineer = await _context.Engineers.FindAsync(id);
        if (engineer == null) throw new NotFoundException(nameof(Engineer), id);

        engineer.FirstName = dto.FirstName;
        engineer.LastName = dto.LastName;
        engineer.MiddleName = dto.MiddleName;
        engineer.QualificationCategory = dto.QualificationCategory;
        engineer.CertificationNumber = dto.CertificationNumber;
        engineer.CertificationExpiryDate = dto.CertificationExpiryDate;
        engineer.Phone = dto.Phone;
        engineer.Email = dto.Email;
        engineer.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var engineer = await _context.Engineers.FindAsync(id);
        if (engineer == null) throw new NotFoundException(nameof(Engineer), id);

        engineer.IsActive = false; // Мягкое отключение
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
