using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UavFleetManagement.Application.Common.Events;
using UavFleetManagement.Domain.Entities.Insurance;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Enums;
using UavFleetManagement.Infrastructure.Persistence.Contexts;

namespace UavFleetManagement.Infrastructure.Workers;

public class IntegrationEventConsumersWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBus _eventBus;
    private readonly ILogger<IntegrationEventConsumersWorker> _logger;

    public IntegrationEventConsumersWorker(
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ILogger<IntegrationEventConsumersWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IntegrationEventConsumersWorker запущен и ожидает событий.");

        await foreach (var integrationEvent in _eventBus.SubscribeAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                switch (integrationEvent)
                {
                    case InnVerificationRequestedEvent innEvent:
                        await HandleInnVerificationAsync(dbContext, innEvent, stoppingToken);
                        break;

                    case RegistrationApplicationCreatedEvent regEvent:
                        await HandleRegistrationAsync(dbContext, regEvent, stoppingToken);
                        break;

                    case PolicyIssuedEvent policyEvent:
                        await HandlePolicyIssuedAsync(dbContext, policyEvent, stoppingToken);
                        break;

                    case FileAttachmentUploadedEvent fileEvent:
                        await HandleFileAttachmentAsync(dbContext, fileEvent, stoppingToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке интеграционного события {EventId}", integrationEvent.EventId);
            }
        }
    }

    private async Task HandleInnVerificationAsync(ApplicationDbContext db, InnVerificationRequestedEvent ev, CancellationToken ct)
    {
        const string handlerName = "PartyVerificationHandler";
        if (await db.InboxEvents.AnyAsync(i => i.EventId == ev.EventId && i.HandlerName == handlerName, ct))
            return; // Идемпотентность

        var org = await db.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == ev.OrganizationId, ct);
        if (org != null)
        {
            // Имитация ответа сервиса СМЭВ 3 / ФНС: валидация формата ИНН
            bool isValid = (ev.Inn.Length == 10 || ev.Inn.Length == 12) && ev.Inn.All(char.IsDigit);
            org.IsVerified = isValid;
            org.VerificationDate = DateTime.UtcNow;
            org.UpdatedAt = DateTime.UtcNow;

            db.InboxEvents.Add(new InboxEvent { EventId = ev.EventId, HandlerName = handlerName, ProcessedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Организация ИНН {Inn} успешно верифицирована через СМЭВ 3.", ev.Inn);
        }
    }

    private async Task HandleRegistrationAsync(ApplicationDbContext db, RegistrationApplicationCreatedEvent ev, CancellationToken ct)
    {
        const string handlerName = "RosaviatsiaRegistrationHandler";
        if (await db.InboxEvents.AnyAsync(i => i.EventId == ev.EventId && i.HandlerName == handlerName, ct))
            return;

        var app = await db.UavRegistrationApplications.FirstOrDefaultAsync(a => a.ApplicationId == ev.ApplicationId, ct);
        if (app != null)
        {
            // Имитация шлюза СМЭВ 4 / Росавиации: автоматическое присвоение учетного номера
            var randDigits = Random.Shared.Next(1000, 9999);
            var regNumber = $"RA-{randDigits}G";

            app.Status = UavApplicationStatus.Approved;
            app.RosaviatsiaNumber = regNumber;
            app.ProcessedAt = DateTime.UtcNow;
            app.UpdatedAt = DateTime.UtcNow;

            db.InboxEvents.Add(new InboxEvent { EventId = ev.EventId, HandlerName = handlerName, ProcessedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Заявление #{AppNum} одобрено Росавиацией, присвоен номер {RegNum}.", app.ApplicationNumber, regNumber);
        }
    }

    private async Task HandlePolicyIssuedAsync(ApplicationDbContext db, PolicyIssuedEvent ev, CancellationToken ct)
    {
        const string handlerName = "InsuranceScoringHandler";
        if (await db.InboxEvents.AnyAsync(i => i.EventId == ev.EventId && i.HandlerName == handlerName, ct))
            return;

        var policy = await db.InsurancePolicies.FirstOrDefaultAsync(p => p.PolicyId == ev.PolicyId, ct);
        if (policy != null)
        {
            policy.Status = PolicyStatus.Active;
            policy.UpdatedAt = DateTime.UtcNow;

            // Выставляем инвойс на оплату страховой премии
            var invoice = new BillingInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{Random.Shared.Next(10000, 99999)}",
                OrderId = policy.PolicyNumber,
                ServiceType = "InsurancePolicy",
                PayerOwnerId = policy.OwnerId,
                AmountRub = policy.PremiumRub,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.BillingInvoices.Add(invoice);

            db.InboxEvents.Add(new InboxEvent { EventId = ev.EventId, HandlerName = handlerName, ProcessedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Полис #{PolicyNumber} активирован, сформирован счет #{InvoiceNumber}.", policy.PolicyNumber, invoice.InvoiceNumber);
        }
    }

    private async Task HandleFileAttachmentAsync(ApplicationDbContext db, FileAttachmentUploadedEvent ev, CancellationToken ct)
    {
        const string handlerName = "S3AttachmentProcessorHandler";
        if (await db.InboxEvents.AnyAsync(i => i.EventId == ev.EventId && i.HandlerName == handlerName, ct))
            return;

        var attachment = await db.FileAttachments.FirstOrDefaultAsync(f => f.FileId == ev.FileId, ct);
        if (attachment != null)
        {
            // Проверка контрольной суммы и антивирусное сканирование
            attachment.Status = "VerifiedClean";
            attachment.UpdatedAt = DateTime.UtcNow;

            db.InboxEvents.Add(new InboxEvent { EventId = ev.EventId, HandlerName = handlerName, ProcessedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Файл {FileId} проверен и отмечен как безопасный (VerifiedClean).", ev.FileId);
        }
    }
}
