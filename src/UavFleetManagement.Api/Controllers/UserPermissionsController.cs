using System.Security.Claims;
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
public class UserPermissionsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UserPermissionsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _context.UserModelPermissions
            .Include(p => p.User)
            .Include(p => p.Model)
            .Select(p => new UserModelPermissionDto(
                p.PermissionId,
                p.UserId,
                p.User != null ? $"{p.User.LastName} {p.User.FirstName}" : string.Empty,
                p.ModelId,
                p.Model != null ? p.Model.ModelName : string.Empty,
                p.GrantedDate,
                p.InsurancePolicyNumber
            ))
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// Просмотр собственных допусков текущего пользователя
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(subClaim, out var credentialId))
        {
            return Unauthorized();
        }

        var cred = await _context.UserCredentials.FindAsync(credentialId);
        if (cred?.LinkedUserId == null)
        {
            return Ok(new List<UserModelPermissionDto>());
        }

        var list = await _context.UserModelPermissions
            .Where(p => p.UserId == cred.LinkedUserId.Value)
            .Include(p => p.User)
            .Include(p => p.Model)
            .Select(p => new UserModelPermissionDto(
                p.PermissionId,
                p.UserId,
                p.User != null ? $"{p.User.LastName} {p.User.FirstName}" : string.Empty,
                p.ModelId,
                p.Model != null ? p.Model.ModelName : string.Empty,
                p.GrantedDate,
                p.InsurancePolicyNumber
            ))
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] UserModelPermissionCreateDto dto)
    {
        var perm = new UserModelPermission
        {
            UserId = dto.UserId,
            ModelId = dto.ModelId,
            GrantedDate = dto.GrantedDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            InsurancePolicyNumber = dto.InsurancePolicyNumber
        };

        _context.UserModelPermissions.Add(perm);
        await _context.SaveChangesAsync();

        return Ok(perm);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Revoke(int id)
    {
        var perm = await _context.UserModelPermissions.FindAsync(id);
        if (perm == null) throw new NotFoundException(nameof(UserModelPermission), id);

        _context.UserModelPermissions.Remove(perm);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
