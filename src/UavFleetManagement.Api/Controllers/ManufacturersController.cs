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
public class ManufacturersController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ManufacturersController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _context.Manufacturers
            .Select(m => new ManufacturerDto(
                m.ManufacturerId,
                m.Name,
                m.Country,
                m.Inn,
                m.Website,
                m.ContactPhone,
                m.CreatedAt
            ))
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _context.Manufacturers.FindAsync(id);
        if (m == null) throw new NotFoundException(nameof(Manufacturer), id);

        return Ok(new ManufacturerDto(
            m.ManufacturerId,
            m.Name,
            m.Country,
            m.Inn,
            m.Website,
            m.ContactPhone,
            m.CreatedAt
        ));
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] ManufacturerCreateDto dto)
    {
        var item = new Manufacturer
        {
            Name = dto.Name,
            Country = dto.Country ?? "Россия",
            Inn = dto.Inn,
            Website = dto.Website,
            ContactPhone = dto.ContactPhone,
            CreatedAt = DateTime.UtcNow
        };

        _context.Manufacturers.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.ManufacturerId }, new ManufacturerDto(
            item.ManufacturerId,
            item.Name,
            item.Country,
            item.Inn,
            item.Website,
            item.ContactPhone,
            item.CreatedAt
        ));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturerCreateDto dto)
    {
        var item = await _context.Manufacturers.FindAsync(id);
        if (item == null) throw new NotFoundException(nameof(Manufacturer), id);

        item.Name = dto.Name;
        item.Country = dto.Country;
        item.Inn = dto.Inn;
        item.Website = dto.Website;
        item.ContactPhone = dto.ContactPhone;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Manufacturers.FindAsync(id);
        if (item == null) throw new NotFoundException(nameof(Manufacturer), id);

        _context.Manufacturers.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
