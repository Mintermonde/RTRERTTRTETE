using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Domain.Entities.Insurance;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public BillingController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Реестр финансовых инвойсов
    /// </summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices()
    {
        var invoices = await _context.BillingInvoices
            .Include(i => i.PayerOwner)
            .Where(i => !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                i.InvoiceId,
                i.InvoiceNumber,
                i.OrderId,
                i.ServiceType,
                i.PayerOwnerId,
                PayerName = i.PayerOwner != null ? i.PayerOwner.Name : null,
                i.AmountRub,
                i.PaymentStatus,
                i.PaymentMethod,
                i.PaymentReference,
                i.PaidAt,
                i.CreatedAt
            })
            .ToListAsync();

        return Ok(invoices);
    }

    /// <summary>
    /// Оплата инвойса
    /// </summary>
    [HttpPost("invoices/{id}/pay")]
    public async Task<IActionResult> PayInvoice(int id, [FromBody] PayInvoiceRequest request)
    {
        var invoice = await _context.BillingInvoices.FirstOrDefaultAsync(i => i.InvoiceId == id && !i.IsDeleted);
        if (invoice == null) throw new NotFoundException(nameof(BillingInvoice), id);

        invoice.PaymentStatus = "Paid";
        invoice.PaymentMethod = request.PaymentMethod ?? "CorporateCard";
        invoice.PaymentReference = $"TX-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
        invoice.PaidAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(invoice);
    }

    /// <summary>
    /// Экспорт финансовых проводок в CSV
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Superuser,Accountant,Admin")]
    public async Task<IActionResult> ExportInvoicesCsv()
    {
        var list = await _context.BillingInvoices
            .Include(i => i.PayerOwner)
            .Where(i => !i.IsDeleted)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID;Номер счета;Основание;Плательщик;Сумма (руб);Статус;Метод;Референс;Дата оплаты;Дата выставления");

        foreach (var i in list)
        {
            sb.AppendLine($"{i.InvoiceId};{i.InvoiceNumber};{i.OrderId};{i.PayerOwner?.Name};{i.AmountRub};{i.PaymentStatus};{i.PaymentMethod};{i.PaymentReference};{i.PaidAt:yyyy-MM-dd HH:mm};{i.CreatedAt:yyyy-MM-dd HH:mm}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv; charset=utf-8", $"billing_invoices_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}

public record PayInvoiceRequest(string? PaymentMethod = "CorporateCard");
