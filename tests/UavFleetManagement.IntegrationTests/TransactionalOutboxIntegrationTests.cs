using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Domain.Entities.Fleet;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Domain.Entities.Registration;
using UavFleetManagement.Domain.Enums;
using UavFleetManagement.Infrastructure.Persistence.Contexts;
using Xunit;

namespace UavFleetManagement.IntegrationTests;

public class TransactionalOutboxIntegrationTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SaveRegistrationApplication_WithOutboxEvent_SavedAtomicallyInPendingStatus()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var owner = new UavOwner
        {
            OwnerId = 1,
            LegalType = OwnerLegalType.LE,
            Name = "ООО Тест Авиа",
            Inn = "7701234567"
        };
        context.UavOwners.Add(owner);
        await context.SaveChangesAsync();

        var app = new UavRegistrationApplication
        {
            ApplicationNumber = "REG-2026-TEST-01",
            ApplicationType = UavApplicationType.InitialRegistration,
            ApplicantOwnerId = owner.OwnerId,
            UavModelName = "Геоскан 401",
            SerialNumber = "TEST-SN-001",
            MtowKg = 9.30m,
            Status = UavApplicationStatus.Pending
        };

        var outboxEvent = new OutboxEvent
        {
            AggregateType = "UavRegistration",
            AggregateId = "1",
            EventType = "RegistrationApplicationCreatedEvent",
            Payload = "{\"test\": true}",
            Metadata = "{\"traceId\": \"123\"}",
            Status = "Pending"
        };

        // Act: атомарная запись в БД
        context.UavRegistrationApplications.Add(app);
        context.OutboxEvents.Add(outboxEvent);
        await context.SaveChangesAsync();

        // Assert
        var savedApp = await context.UavRegistrationApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == "REG-2026-TEST-01");
        savedApp.Should().NotBeNull();
        savedApp!.Status.Should().Be(UavApplicationStatus.Pending);

        var savedOutbox = await context.OutboxEvents.FirstOrDefaultAsync(e => e.AggregateType == "UavRegistration");
        savedOutbox.Should().NotBeNull();
        savedOutbox!.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task OutboxWorkerSimulation_TransitionsPendingToProcessingToPublished()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var outboxEvent = new OutboxEvent
        {
            EventId = Guid.NewGuid(),
            AggregateType = "InsurancePolicy",
            AggregateId = "42",
            EventType = "PolicyIssuedEvent",
            Payload = "{}",
            Metadata = "{}",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        context.OutboxEvents.Add(outboxEvent);
        await context.SaveChangesAsync();

        // Act: симуляция выборки воркером
        var pending = await context.OutboxEvents.Where(e => e.Status == "Pending").FirstOrDefaultAsync();
        pending.Should().NotBeNull();

        // Шаг 1: перевод в Processing
        pending!.Status = "Processing";
        pending.ProcessedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        var processingEvent = await context.OutboxEvents.FirstOrDefaultAsync(e => e.EventId == outboxEvent.EventId);
        processingEvent!.Status.Should().Be("Processing");

        // Шаг 2: перевод в Published
        processingEvent.Status = "Published";
        await context.SaveChangesAsync();

        // Assert
        var publishedEvent = await context.OutboxEvents.FirstOrDefaultAsync(e => e.EventId == outboxEvent.EventId);
        publishedEvent!.Status.Should().Be("Published");
    }
}
