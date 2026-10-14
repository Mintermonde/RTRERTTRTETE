using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace UavFleetManagement.IntegrationTests;

public class ZeroTrustSecurityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ZeroTrustSecurityIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/api/uavmodels")]
    [InlineData("/api/uavinstances")]
    [InlineData("/api/manufacturers")]
    [InlineData("/api/organizations")]
    [InlineData("/api/registration/applications")]
    [InlineData("/api/insurance/policies")]
    [InlineData("/api/maintenancelogs")]
    [InlineData("/api/componentscatalog")]
    [InlineData("/api/billing/invoices")]
    public async Task AnonymousAccess_ToDataEndpoints_MustBeStrictlyUnauthorized(string endpoint)
    {
        // Act: попытка неавторизованного запроса к защищенной БД через браузер/клиент
        var response = await _client.GetAsync(endpoint);

        // Assert: доступ полностью запрещен (401 Unauthorized)
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            $"Эндпоинт {endpoint} должен быть полностью защищен от неавторизованного доступа к БД");
    }
}
