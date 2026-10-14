using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UavFleetManagement.Application.Common.Events;
using UavFleetManagement.Infrastructure.Persistence.Contexts;

namespace UavFleetManagement.Infrastructure.Workers;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceProvider serviceProvider,
        IEventBus eventBus,
        ILogger<OutboxPublisherWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _eventBus = eventBus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisherWorker запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (await dbContext.Database.CanConnectAsync(stoppingToken))
                {
                    var pendingEvents = await dbContext.OutboxEvents
                        .Where(e => e.Status == "Pending")
                        .OrderBy(e => e.CreatedAt)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    if (pendingEvents.Any())
                    {
                        foreach (var outboxEvent in pendingEvents)
                        {
                            try
                            {
                                outboxEvent.Status = "Processing";
                                outboxEvent.ProcessedAt = DateTime.UtcNow;

                                var domainEvent = DeserializeEvent(outboxEvent.EventType, outboxEvent.Payload);
                                if (domainEvent != null)
                                {
                                    await _eventBus.PublishAsync(domainEvent, stoppingToken);
                                }

                                outboxEvent.Status = "Published";
                                _logger.LogInformation("Outbox событие {EventId} [{EventType}] успешно опубликовано.", outboxEvent.EventId, outboxEvent.EventType);
                            }
                            catch (Exception ex)
                            {
                                outboxEvent.Status = "Failed";
                                outboxEvent.RetryCount++;
                                outboxEvent.LastError = ex.Message;
                                _logger.LogError(ex, "Ошибка при публикации Outbox события {EventId}", outboxEvent.EventId);
                            }
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Предупреждение в OutboxPublisherWorker: {Message}", ex.Message);
            }

            await Task.Delay(2000, stoppingToken);
        }
    }

    private static IntegrationEvent? DeserializeEvent(string eventType, string payload)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return eventType switch
        {
            nameof(RegistrationApplicationCreatedEvent) => JsonSerializer.Deserialize<RegistrationApplicationCreatedEvent>(payload, options),
            nameof(InnVerificationRequestedEvent) => JsonSerializer.Deserialize<InnVerificationRequestedEvent>(payload, options),
            nameof(PolicyIssuedEvent) => JsonSerializer.Deserialize<PolicyIssuedEvent>(payload, options),
            nameof(FileAttachmentUploadedEvent) => JsonSerializer.Deserialize<FileAttachmentUploadedEvent>(payload, options),
            _ => null
        };
    }
}
