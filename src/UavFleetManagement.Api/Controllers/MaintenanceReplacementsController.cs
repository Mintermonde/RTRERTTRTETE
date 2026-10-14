using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Maintenance;
using UavFleetManagement.Domain.Entities.Maintenance;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceReplacementsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public MaintenanceReplacementsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-log/{logId}")]
    public async Task<IActionResult> GetByLog(int logId)
    {
        var items = await _context.MaintenanceComponentReplacements
            .Where(r => r.LogId == logId)
            .Include(r => r.RemovedComponent)
            .Include(r => r.InstalledComponent)
            .Select(r => new MaintenanceReplacementDto(
                r.ReplacementId,
                r.LogId,
                r.RemovedComponentId,
                r.RemovedComponent != null ? r.RemovedComponent.ComponentSerialNumber : null,
                r.InstalledComponentId,
                r.InstalledComponent != null ? r.InstalledComponent.ComponentSerialNumber : null,
                r.ReasonForReplacement
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] MaintenanceReplacementCreateDto dto)
    {
        var replacement = new MaintenanceComponentReplacement
        {
            LogId = dto.LogId,
            RemovedComponentId = dto.RemovedComponentId,
            InstalledComponentId = dto.InstalledComponentId,
            ReasonForReplacement = dto.ReasonForReplacement
        };

        _context.MaintenanceComponentReplacements.Add(replacement);
        await _context.SaveChangesAsync();

        return Ok(replacement);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var replacement = await _context.MaintenanceComponentReplacements.FindAsync(id);
        if (replacement == null) throw new NotFoundException(nameof(MaintenanceComponentReplacement), id);

        _context.MaintenanceComponentReplacements.Remove(replacement);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
