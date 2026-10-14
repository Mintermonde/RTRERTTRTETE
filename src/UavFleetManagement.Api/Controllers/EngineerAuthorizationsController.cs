using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Domain.Entities.Engineers;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EngineerAuthorizationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public EngineerAuthorizationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получение матрицы допусков инженера («что может ремонтировать»)
    /// </summary>
    [HttpGet("{engineerId}")]
    public async Task<IActionResult> GetByEngineer(int engineerId)
    {
        var auths = await _context.EngineerRepairAuthorizations
            .Include(a => a.AllowedUavModel)
            .Include(a => a.Engineer)
            .Where(a => a.EngineerId == engineerId)
            .Select(a => new
            {
                a.AuthId,
                a.EngineerId,
                EngineerName = a.Engineer != null ? $"{a.Engineer.LastName} {a.Engineer.FirstName}" : null,
                a.AllowedUavModelId,
                AllowedModelName = a.AllowedUavModel != null ? a.AllowedUavModel.ModelName : "Все модели данного двигателя",
                a.AllowedEngineType,
                a.MaintenanceCategories,
                a.CertificateEndorsementCode,
                a.ValidUntil,
                a.IsActive
            })
            .ToListAsync();

        return Ok(auths);
    }

    /// <summary>
    /// Выдача нового допуска по сертификату инженера (Администратор)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Superuser,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEngineerAuthRequest request)
    {
        var engineer = await _context.Engineers.FirstOrDefaultAsync(e => e.EngineerId == request.EngineerId);
        if (engineer == null) throw new NotFoundException(nameof(Engineer), request.EngineerId);

        var auth = new EngineerRepairAuthorization
        {
            EngineerId = request.EngineerId,
            AllowedUavModelId = request.AllowedUavModelId,
            AllowedEngineType = request.AllowedEngineType ?? "All",
            MaintenanceCategories = request.MaintenanceCategories ?? "TO-1,TO-2,Seasonal",
            CertificateEndorsementCode = request.CertificateEndorsementCode,
            ValidUntil = request.ValidUntil,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.EngineerRepairAuthorizations.Add(auth);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByEngineer), new { engineerId = auth.EngineerId }, auth);
    }
}

public record CreateEngineerAuthRequest(
    int EngineerId,
    int? AllowedUavModelId,
    string? AllowedEngineType,
    string? MaintenanceCategories,
    string CertificateEndorsementCode,
    DateOnly ValidUntil
);
