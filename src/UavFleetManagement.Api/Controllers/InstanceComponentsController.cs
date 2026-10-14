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
public class InstanceComponentsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public InstanceComponentsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-instance/{instanceId}")]
    public async Task<IActionResult> GetByInstance(int instanceId)
    {
        var items = await _context.InstanceComponents
            .Where(ic => ic.InstanceId == instanceId && ic.IsActive)
            .Include(ic => ic.ComponentType)
            .Select(ic => new InstanceComponentDto(
                ic.InstalledComponentId,
                ic.InstanceId,
                ic.ComponentTypeId,
                ic.ComponentType != null ? ic.ComponentType.ComponentName : string.Empty,
                ic.ComponentSerialNumber,
                ic.InstallationDate,
                ic.ComponentOperatingHours,
                ic.IsActive,
                ic.Condition,
                ic.BatteryChargeCyclesInitial
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Install([FromBody] InstanceComponentInstallDto dto)
    {
        var component = new InstanceComponent
        {
            InstanceId = dto.InstanceId,
            ComponentTypeId = dto.ComponentTypeId,
            ComponentSerialNumber = dto.ComponentSerialNumber,
            InstallationDate = dto.InstallationDate,
            ComponentOperatingHours = dto.ComponentOperatingHours,
            Condition = dto.Condition,
            BatteryChargeCyclesInitial = dto.BatteryChargeCyclesInitial,
            IsActive = true
        };

        _context.InstanceComponents.Add(component);
        await _context.SaveChangesAsync();

        return Ok(component);
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var item = await _context.InstanceComponents.FindAsync(id);
        if (item == null) throw new NotFoundException(nameof(InstanceComponent), id);

        item.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
