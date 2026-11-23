using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Virtues.GetVirtues;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Features.Virtues;

public class GetVirtuesTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetVirtues_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Virtues.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetVirtues_WhenAuthenticatedWithAnyRole_ShouldReturnSuccess(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var virtue = await CreateVirtueAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.Virtues.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Virtues.Should().Contain(item => item.Id == virtue.Id);
    }

    // --- 2. Happy Path ---
    [Fact]
    public async Task GetVirtues_WhenAuthenticated_ShouldReturnVirtuesOrderedAlphabeticallyByName()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var firstVirtue = await CreateVirtueAsync(name: "A-Virtue");
        var secondVirtue = await CreateVirtueAsync(name: "Z-Virtue");

        // Act
        var response = await Client.GetAsync(ApiRoutes.Virtues.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Virtues.Select(virtue => virtue.Name).Should().BeInAscendingOrder();

        payload.Virtues.Should().Contain(virtue =>
            virtue.Id == firstVirtue.Id &&
            virtue.Name == firstVirtue.Name);

        payload.Virtues.Should().Contain(virtue =>
            virtue.Id == secondVirtue.Id &&
            virtue.Name == secondVirtue.Name);
    }
}
