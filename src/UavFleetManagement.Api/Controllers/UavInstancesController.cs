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
public class UavInstancesController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UavInstancesController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        bool isSuperuser = User.IsInRole("Superuser");

        var instances = await _context.UavInstances
            .Include(i => i.Model)
            .Include(i => i.Owner)
            .Select(i => new UavInstanceDto
            {
                InstanceId = i.InstanceId,
                ModelId = i.ModelId,
                ModelName = i.Model != null ? i.Model.ModelName : string.Empty,
                OwnerId = i.OwnerId,
                OwnerName = i.Owner != null ? i.Owner.Name : null,
                SerialNumber = i.SerialNumber,
                RosaviatsiaNumber = i.RosaviatsiaNumber,
                ProductionDate = i.ProductionDate,
                TotalFlightTimeHours = i.TotalFlightTimeHours,
                CurrentStatus = i.CurrentStatus,
                PrimaryColor = i.PrimaryColor,
                LastUpdate = i.LastUpdate,
                // Страховой полис доступен только Superuser
                HullInsurancePolicy = isSuperuser ? i.HullInsurancePolicy : null,
                InsuranceExpiryDate = isSuperuser ? i.InsuranceExpiryDate : null
            })
            .ToListAsync();

        return Ok(instances);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var i = await _context.UavInstances
            .Include(x => x.Model)
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.InstanceId == id);

        if (i == null) throw new NotFoundException(nameof(UavInstance), id);

        bool isSuperuser = User.IsInRole("Superuser");
        return Ok(new UavInstanceDto
        {
            InstanceId = i.InstanceId,
            ModelId = i.ModelId,
            ModelName = i.Model?.ModelName ?? string.Empty,
            OwnerId = i.OwnerId,
            OwnerName = i.Owner?.Name,
            SerialNumber = i.SerialNumber,
            RosaviatsiaNumber = i.RosaviatsiaNumber,
            ProductionDate = i.ProductionDate,
            TotalFlightTimeHours = i.TotalFlightTimeHours,
            CurrentStatus = i.CurrentStatus,
            PrimaryColor = i.PrimaryColor,
            LastUpdate = i.LastUpdate,
            HullInsurancePolicy = isSuperuser ? i.HullInsurancePolicy : null,
            InsuranceExpiryDate = isSuperuser ? i.InsuranceExpiryDate : null
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] UavInstanceCreateDto dto)
    {
        var instance = new UavInstance
        {
            ModelId = dto.ModelId,
            OwnerId = dto.OwnerId,
            SerialNumber = dto.SerialNumber,
            RosaviatsiaNumber = dto.RosaviatsiaNumber,
            ProductionDate = dto.ProductionDate,
            TotalFlightTimeHours = dto.TotalFlightTimeHours,
            CurrentStatus = dto.CurrentStatus,
            HullInsurancePolicy = dto.HullInsurancePolicy,
            InsuranceExpiryDate = dto.InsuranceExpiryDate,
            PrimaryColor = dto.PrimaryColor,
            LastUpdate = DateTime.UtcNow
        };

        _context.UavInstances.Add(instance);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = instance.InstanceId }, instance);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] UavInstanceCreateDto dto)
    {
        var instance = await _context.UavInstances.FindAsync(id);
        if (instance == null) throw new NotFoundException(nameof(UavInstance), id);

        instance.ModelId = dto.ModelId;
        instance.OwnerId = dto.OwnerId;
        instance.SerialNumber = dto.SerialNumber;
        instance.RosaviatsiaNumber = dto.RosaviatsiaNumber;
        instance.ProductionDate = dto.ProductionDate;
        instance.TotalFlightTimeHours = dto.TotalFlightTimeHours;
        instance.CurrentStatus = dto.CurrentStatus;
        instance.HullInsurancePolicy = dto.HullInsurancePolicy;
        instance.InsuranceExpiryDate = dto.InsuranceExpiryDate;
        instance.PrimaryColor = dto.PrimaryColor;
        instance.LastUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UavStatusUpdateDto dto)
    {
        var instance = await _context.UavInstances.FindAsync(id);
        if (instance == null) throw new NotFoundException(nameof(UavInstance), id);

        instance.CurrentStatus = dto.Status;
        instance.LastUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var instance = await _context.UavInstances.FindAsync(id);
        if (instance == null) throw new NotFoundException(nameof(UavInstance), id);

        _context.UavInstances.Remove(instance);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
