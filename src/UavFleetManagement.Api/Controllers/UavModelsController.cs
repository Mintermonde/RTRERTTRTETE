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
public class UavModelsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public UavModelsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получение списка моделей. Обычные пользователи получают сводную информацию без цены.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var isSuperuser = User.IsInRole("Superuser");

        if (isSuperuser)
        {
            var adminList = await _context.UavModels
                .Include(m => m.Manufacturer)
                .Select(m => new UavModelDetailsDto
                {
                    ModelId = m.ModelId,
                    ManufacturerId = m.ManufacturerId,
                    ModelName = m.ModelName,
                    ManufacturerName = m.Manufacturer != null ? m.Manufacturer.Name : string.Empty,
                    AerodynamicSchema = m.AerodynamicSchema.ToString(),
                    WeightCategory = m.WeightCategory.ToString(),
                    ControlType = m.ControlType.ToString(),
                    MtowKg = m.MtowKg,
                    MaxFlightTimeMins = m.MaxFlightTimeMins,
                    MaxRangeKm = m.MaxRangeKm,
                    BasePriceRub = m.BasePriceRub, // Открыто только для Superuser
                    CruisingSpeedKmh = m.CruisingSpeedKmh,
                    MaxAltitudeM = m.MaxAltitudeM,
                    MaxWindSpeedMs = m.MaxWindSpeedMs,
                    MinTempCelsius = m.MinTempCelsius,
                    MaxTempCelsius = m.MaxTempCelsius,
                    LengthMm = m.LengthMm,
                    WidthWingspanMm = m.WidthWingspanMm,
                    HeightMm = m.HeightMm
                })
                .ToListAsync();

            return Ok(adminList);
        }

        // Для обычного пользователя - базовая сводная информация без цен
        var userList = await _context.UavModels
            .Include(m => m.Manufacturer)
            .Select(m => new UavModelSummaryDto
            {
                ModelId = m.ModelId,
                ManufacturerId = m.ManufacturerId,
                ModelName = m.ModelName,
                ManufacturerName = m.Manufacturer != null ? m.Manufacturer.Name : string.Empty,
                AerodynamicSchema = m.AerodynamicSchema.ToString(),
                WeightCategory = m.WeightCategory.ToString(),
                ControlType = m.ControlType.ToString(),
                MtowKg = m.MtowKg,
                MaxFlightTimeMins = m.MaxFlightTimeMins,
                MaxRangeKm = m.MaxRangeKm
            })
            .ToListAsync();

        return Ok(userList);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _context.UavModels
            .Include(x => x.Manufacturer)
            .FirstOrDefaultAsync(x => x.ModelId == id);

        if (m == null) throw new NotFoundException(nameof(UavModel), id);

        var isSuperuser = User.IsInRole("Superuser");
        if (isSuperuser)
        {
            return Ok(new UavModelDetailsDto
            {
                ModelId = m.ModelId,
                ManufacturerId = m.ManufacturerId,
                ModelName = m.ModelName,
                ManufacturerName = m.Manufacturer?.Name ?? string.Empty,
                AerodynamicSchema = m.AerodynamicSchema.ToString(),
                WeightCategory = m.WeightCategory.ToString(),
                ControlType = m.ControlType.ToString(),
                MtowKg = m.MtowKg,
                MaxFlightTimeMins = m.MaxFlightTimeMins,
                MaxRangeKm = m.MaxRangeKm,
                BasePriceRub = m.BasePriceRub,
                CruisingSpeedKmh = m.CruisingSpeedKmh,
                MaxAltitudeM = m.MaxAltitudeM,
                MaxWindSpeedMs = m.MaxWindSpeedMs,
                MinTempCelsius = m.MinTempCelsius,
                MaxTempCelsius = m.MaxTempCelsius,
                LengthMm = m.LengthMm,
                WidthWingspanMm = m.WidthWingspanMm,
                HeightMm = m.HeightMm
            });
        }

        return Ok(new UavModelSummaryDto
        {
            ModelId = m.ModelId,
            ManufacturerId = m.ManufacturerId,
            ModelName = m.ModelName,
            ManufacturerName = m.Manufacturer?.Name ?? string.Empty,
            AerodynamicSchema = m.AerodynamicSchema.ToString(),
            WeightCategory = m.WeightCategory.ToString(),
            ControlType = m.ControlType.ToString(),
            MtowKg = m.MtowKg,
            MaxFlightTimeMins = m.MaxFlightTimeMins,
            MaxRangeKm = m.MaxRangeKm
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Create([FromBody] UavModelCreateDto dto)
    {
        var model = new UavModel
        {
            ManufacturerId = dto.ManufacturerId,
            ModelName = dto.ModelName,
            AerodynamicSchema = dto.AerodynamicSchema,
            WeightCategory = dto.WeightCategory,
            ControlType = dto.ControlType,
            MtowKg = dto.MtowKg,
            MaxFlightTimeMins = dto.MaxFlightTimeMins,
            MaxRangeKm = dto.MaxRangeKm,
            CruisingSpeedKmh = dto.CruisingSpeedKmh,
            MaxAltitudeM = dto.MaxAltitudeM,
            MaxWindSpeedMs = dto.MaxWindSpeedMs,
            MinTempCelsius = dto.MinTempCelsius,
            MaxTempCelsius = dto.MaxTempCelsius,
            LengthMm = dto.LengthMm,
            WidthWingspanMm = dto.WidthWingspanMm,
            HeightMm = dto.HeightMm,
            BasePriceRub = dto.BasePriceRub
        };

        _context.UavModels.Add(model);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = model.ModelId }, model);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Update(int id, [FromBody] UavModelCreateDto dto)
    {
        var model = await _context.UavModels.FindAsync(id);
        if (model == null) throw new NotFoundException(nameof(UavModel), id);

        model.ManufacturerId = dto.ManufacturerId;
        model.ModelName = dto.ModelName;
        model.AerodynamicSchema = dto.AerodynamicSchema;
        model.WeightCategory = dto.WeightCategory;
        model.ControlType = dto.ControlType;
        model.MtowKg = dto.MtowKg;
        model.MaxFlightTimeMins = dto.MaxFlightTimeMins;
        model.MaxRangeKm = dto.MaxRangeKm;
        model.CruisingSpeedKmh = dto.CruisingSpeedKmh;
        model.MaxAltitudeM = dto.MaxAltitudeM;
        model.MaxWindSpeedMs = dto.MaxWindSpeedMs;
        model.MinTempCelsius = dto.MinTempCelsius;
        model.MaxTempCelsius = dto.MaxTempCelsius;
        model.LengthMm = dto.LengthMm;
        model.WidthWingspanMm = dto.WidthWingspanMm;
        model.HeightMm = dto.HeightMm;
        model.BasePriceRub = dto.BasePriceRub;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _context.UavModels.FindAsync(id);
        if (model == null) throw new NotFoundException(nameof(UavModel), id);

        _context.UavModels.Remove(model);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
