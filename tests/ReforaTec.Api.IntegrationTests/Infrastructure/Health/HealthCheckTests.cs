using System.Net;
using FluentAssertions;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Infrastructure.Health;

public class HealthCheckTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // Security: N/A — anonymous health probe endpoint

    [Fact]
    public async Task Health_WhenApiIsRunning_ShouldReturnHealthy()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Health);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }
}
