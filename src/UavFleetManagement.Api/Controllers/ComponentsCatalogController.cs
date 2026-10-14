using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Catalog;
using UavFleetManagement.Domain.Entities.Catalog;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComponentsCatalogController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ComponentsCatalogController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _context.ComponentsCatalog
            .Include(c => c.Manufacturer)
            .Select(c => new ComponentCatalogDto(
                c.ComponentTypeId,
                c.ManufacturerId,
                c.Manufacturer != null ? c.Manufacturer.Name : string.Empty,
                c.PartNumber,
                c.ComponentName,
                c.Category,
                c.MtbfHours,
                c.WeightG,
                c.SpecificationsJson
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await _context.ComponentsCatalog
            .Include(x => x.Manufacturer)
            .FirstOrDefaultAsync(x => x.ComponentTypeId == id);

        if (c == null) throw new NotFoundException(nameof(ComponentCatalog), id);

        return Ok(new ComponentCatalogDto(
            c.ComponentTypeId,
            c.ManufacturerId,
            c.Manufacturer?.Name ?? string.Empty,
            c.PartNumber,
            c.ComponentName,
            c.Category,
            c.MtbfHours,
            c.WeightG,
            c.SpecificationsJson
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] ComponentCatalogCreateDto dto)
    {
        var item = new ComponentCatalog
        {
            ManufacturerId = dto.ManufacturerId,
            PartNumber = dto.PartNumber,
            ComponentName = dto.ComponentName,
            Category = dto.Category,
            MtbfHours = dto.MtbfHours,
            WeightG = dto.WeightG,
            SpecificationsJson = dto.SpecificationsJson
        };

        _context.ComponentsCatalog.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.ComponentTypeId }, item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] ComponentCatalogCreateDto dto)
    {
        var item = await _context.ComponentsCatalog.FindAsync(id);
        if (item == null) throw new NotFoundException(nameof(ComponentCatalog), id);

        item.ManufacturerId = dto.ManufacturerId;
        item.PartNumber = dto.PartNumber;
        item.ComponentName = dto.ComponentName;
        item.Category = dto.Category;
        item.MtbfHours = dto.MtbfHours;
        item.WeightG = dto.WeightG;
        item.SpecificationsJson = dto.SpecificationsJson;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ComponentsCatalog.FindAsync(id);
        if (item == null) throw new NotFoundException(nameof(ComponentCatalog), id);

        _context.ComponentsCatalog.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
