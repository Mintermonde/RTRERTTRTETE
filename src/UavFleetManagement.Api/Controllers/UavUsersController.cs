using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Pilots;
using UavFleetManagement.Domain.Entities.Pilots;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UavUsersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UavUsersController(IApplicationDbContext context)
    {
        _context = context;
    }

    private static string? MaskLicense(string? license)
    {
        if (string.IsNullOrWhiteSpace(license)) return null;
        if (license.Length <= 4) return "****";
        return license[..2] + new string('*', license.Length - 4) + license[^2..];
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        bool isSuperuser = User.IsInRole("Superuser");

        if (isSuperuser)
        {
            var adminList = await _context.UavUsers
                .Select(u => new UavUserAdminDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    PilotLicenseNumber = u.PilotLicenseNumber,
                    LicenseIssueDate = u.LicenseIssueDate,
                    LicenseExpiryDate = u.LicenseExpiryDate,
                    MedicalCertificateNumber = u.MedicalCertificateNumber,
                    Phone = u.Phone,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    MaskedLicenseNumber = u.PilotLicenseNumber
                })
                .ToListAsync();

            return Ok(adminList);
        }

        // Обычные пользователи видят список с маскированными номерами лицензий
        var publicList = await _context.UavUsers
            .Select(u => new UavUserPublicDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                MiddleName = u.MiddleName,
                MaskedLicenseNumber = MaskLicense(u.PilotLicenseNumber),
                IsActive = u.IsActive
            })
            .ToListAsync();

        return Ok(publicList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var u = await _context.UavUsers.FindAsync(id);
        if (u == null) throw new NotFoundException(nameof(UavUser), id);

        bool isSuperuser = User.IsInRole("Superuser");
        if (isSuperuser)
        {
            return Ok(new UavUserAdminDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                MiddleName = u.MiddleName,
                PilotLicenseNumber = u.PilotLicenseNumber,
                LicenseIssueDate = u.LicenseIssueDate,
                LicenseExpiryDate = u.LicenseExpiryDate,
                MedicalCertificateNumber = u.MedicalCertificateNumber,
                Phone = u.Phone,
                Email = u.Email,
                IsActive = u.IsActive,
                MaskedLicenseNumber = u.PilotLicenseNumber
            });
        }

        return Ok(new UavUserPublicDto
        {
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            MiddleName = u.MiddleName,
            MaskedLicenseNumber = MaskLicense(u.PilotLicenseNumber),
            IsActive = u.IsActive
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] UavUserCreateDto dto)
    {
        var pilot = new UavUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            PilotLicenseNumber = dto.PilotLicenseNumber,
            LicenseIssueDate = dto.LicenseIssueDate,
            LicenseExpiryDate = dto.LicenseExpiryDate,
            MedicalCertificateNumber = dto.MedicalCertificateNumber,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = dto.IsActive
        };

        _context.UavUsers.Add(pilot);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = pilot.UserId }, pilot);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] UavUserCreateDto dto)
    {
        var pilot = await _context.UavUsers.FindAsync(id);
        if (pilot == null) throw new NotFoundException(nameof(UavUser), id);

        pilot.FirstName = dto.FirstName;
        pilot.LastName = dto.LastName;
        pilot.MiddleName = dto.MiddleName;
        pilot.PilotLicenseNumber = dto.PilotLicenseNumber;
        pilot.LicenseIssueDate = dto.LicenseIssueDate;
        pilot.LicenseExpiryDate = dto.LicenseExpiryDate;
        pilot.MedicalCertificateNumber = dto.MedicalCertificateNumber;
        pilot.Phone = dto.Phone;
        pilot.Email = dto.Email;
        pilot.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var pilot = await _context.UavUsers.FindAsync(id);
        if (pilot == null) throw new NotFoundException(nameof(UavUser), id);

        _context.UavUsers.Remove(pilot);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
