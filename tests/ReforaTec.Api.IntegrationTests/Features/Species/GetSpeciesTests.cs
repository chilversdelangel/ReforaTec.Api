using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Species.GetSpecies;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Features.Species;

public class GetSpeciesTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetSpecies_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Species.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetSpecies_WhenAuthenticatedWithAnyRole_ShouldReturnSuccess(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var species = await CreateSpeciesAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.Species.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Species.Should().Contain(item => item.Id == species.Id);
    }

    // --- 2. Happy Path ---
    [Fact]
    public async Task GetSpecies_WhenAuthenticated_ShouldReturnSpeciesOrderedAlphabeticallyByCommonName()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var firstSpecies = await CreateSpeciesAsync(commonName: "A-Species");
        var secondSpecies = await CreateSpeciesAsync(commonName: "Z-Species");

        // Act
        var response = await Client.GetAsync(ApiRoutes.Species.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Species.Select(species => species.CommonName).Should().BeInAscendingOrder();

        payload.Species.Should().Contain(species =>
            species.Id == firstSpecies.Id &&
            species.CommonName == firstSpecies.CommonName);

        payload.Species.Should().Contain(species =>
            species.Id == secondSpecies.Id &&
            species.CommonName == secondSpecies.CommonName);
    }
}
