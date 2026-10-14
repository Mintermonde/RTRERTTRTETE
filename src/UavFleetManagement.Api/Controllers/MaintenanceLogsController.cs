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
public class MaintenanceLogsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public MaintenanceLogsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var logs = await _context.MaintenanceLogs
            .Include(l => l.Instance)
            .Include(l => l.Engineer)
            .OrderByDescending(l => l.MaintenanceDate)
            .Select(l => new MaintenanceLogDto(
                l.LogId,
                l.InstanceId,
                l.Instance != null ? l.Instance.SerialNumber : string.Empty,
                l.TypeOfMaintenance,
                l.MaintenanceDate,
                l.UavOperatingHoursAtMaintenance,
                l.Description,
                l.EngineerId,
                l.Engineer != null ? $"{l.Engineer.LastName} {l.Engineer.FirstName}" : l.EngineerNameFallback,
                l.Engineer != null ? l.Engineer.QualificationCategory : null,
                l.NextMaintenanceDate
            ))
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("by-instance/{instanceId}")]
    public async Task<IActionResult> GetByInstance(int instanceId)
    {
        var logs = await _context.MaintenanceLogs
            .Where(l => l.InstanceId == instanceId)
            .Include(l => l.Instance)
            .Include(l => l.Engineer)
            .Select(l => new MaintenanceLogDto(
                l.LogId,
                l.InstanceId,
                l.Instance != null ? l.Instance.SerialNumber : string.Empty,
                l.TypeOfMaintenance,
                l.MaintenanceDate,
                l.UavOperatingHoursAtMaintenance,
                l.Description,
                l.EngineerId,
                l.Engineer != null ? $"{l.Engineer.LastName} {l.Engineer.FirstName}" : l.EngineerNameFallback,
                l.Engineer != null ? l.Engineer.QualificationCategory : null,
                l.NextMaintenanceDate
            ))
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var l = await _context.MaintenanceLogs
            .Include(x => x.Instance)
            .Include(x => x.Engineer)
            .FirstOrDefaultAsync(x => x.LogId == id);

        if (l == null) throw new NotFoundException(nameof(MaintenanceLog), id);

        return Ok(new MaintenanceLogDto(
            l.LogId,
            l.InstanceId,
            l.Instance != null ? l.Instance.SerialNumber : string.Empty,
            l.TypeOfMaintenance,
            l.MaintenanceDate,
            l.UavOperatingHoursAtMaintenance,
            l.Description,
            l.EngineerId,
            l.Engineer != null ? $"{l.Engineer.LastName} {l.Engineer.FirstName}" : l.EngineerNameFallback,
            l.Engineer != null ? l.Engineer.QualificationCategory : null,
            l.NextMaintenanceDate
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] MaintenanceLogCreateDto dto)
    {
        var log = new MaintenanceLog
        {
            InstanceId = dto.InstanceId,
            TypeOfMaintenance = dto.TypeOfMaintenance,
            MaintenanceDate = dto.MaintenanceDate,
            UavOperatingHoursAtMaintenance = dto.UavOperatingHoursAtMaintenance,
            Description = dto.Description,
            EngineerId = dto.EngineerId,
            EngineerNameFallback = dto.EngineerNameFallback ?? string.Empty,
            NextMaintenanceDate = dto.NextMaintenanceDate
        };

        _context.MaintenanceLogs.Add(log);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = log.LogId }, log);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var log = await _context.MaintenanceLogs.FindAsync(id);
        if (log == null) throw new NotFoundException(nameof(MaintenanceLog), id);

        _context.MaintenanceLogs.Remove(log);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
