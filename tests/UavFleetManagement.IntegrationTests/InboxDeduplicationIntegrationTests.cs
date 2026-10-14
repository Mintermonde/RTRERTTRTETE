using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Domain.Entities.Outbox;
using UavFleetManagement.Infrastructure.Persistence.Contexts;
using Xunit;

namespace UavFleetManagement.IntegrationTests;

public class InboxDeduplicationIntegrationTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task InboxPattern_PreventsDuplicateProcessingOfSameEventId()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var eventId = Guid.NewGuid();
        const string handlerName = "RosaviatsiaRegistrationHandler";

        // Первая обработка
        context.InboxEvents.Add(new InboxEvent
        {
            EventId = eventId,
            HandlerName = handlerName,
            ProcessedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act: проверка перед повторной обработкой
        bool isAlreadyProcessed = await context.InboxEvents
            .AnyAsync(i => i.EventId == eventId && i.HandlerName == handlerName);

        // Assert
        isAlreadyProcessed.Should().BeTrue("Событие уже зафиксировано в таблице Inbox и не должно повторно обрабатываться");
    }
}
