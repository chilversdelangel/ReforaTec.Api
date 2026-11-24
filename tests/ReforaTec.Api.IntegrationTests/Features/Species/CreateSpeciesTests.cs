using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Species.CreateSpecies;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Species;

public class CreateSpeciesTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string ValidImageUrl = "https://cdn.reforatec.org/species/quercus.jpg";

    // --- 1. Security ---
    [Fact]
    public async Task CreateSpecies_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(
            "Quercus virginiana",
            "Encino Siempreverde",
            "Árbol perennifolio de copa amplia.",
            ValidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    public async Task CreateSpecies_WhenAuthenticatedAsNonAdmin_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(
            "Quercus virginiana",
            "Encino Siempreverde",
            "Árbol perennifolio de copa amplia.",
            ValidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateSpecies_WhenScientificNameIsInvalid_ShouldReturnValidationProblem(string invalidScientificName)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var request = new Request(
            invalidScientificName,
            "Encino",
            "Descripción",
            ValidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.ScientificName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateSpecies_WhenCommonNameIsInvalid_ShouldReturnValidationProblem(string invalidCommonName)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var request = new Request(
            "Quercus alba",
            invalidCommonName,
            "Descripción",
            ValidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.CommonName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateSpecies_WhenImageUrlIsInvalid_ShouldReturnValidationProblem(string invalidImageUrl)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var request = new Request(
            "Quercus alba",
            "Encino",
            "Descripción",
            invalidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.ImageUrl));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateSpecies_WhenScientificNameAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        var scientificName = $"Pinus-{Guid.NewGuid().ToString("N")[..6]}";
        await CreateSpeciesAsync(scientificName: scientificName, commonName: "Pino");
        Authenticate(admin);

        var upperScientificName = scientificName.ToUpperInvariant();
        var request = new Request(
            $"  {upperScientificName}  ",
            "Pino",
            "Árbol conífero.",
            ValidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.DuplicateScientificName(upperScientificName));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task CreateSpecies_WhenValid_ShouldPersistAndReturnCreated()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var scientificName = $"Tabebuia-{Guid.NewGuid().ToString("N")[..6]}";
        const string commonName = "Guayacán";
        const string description = "Árbol ornamental de floración amarilla.";
        var request = new Request(
            $"  {scientificName}  ",
            $"  {commonName}  ",
            $"  {description}  ",
            ValidImageUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Species.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().BeGreaterThan(0);
        payload.ScientificName.Should().Be(scientificName.ToSanitized());
        payload.CommonName.Should().Be(commonName.ToSanitized());
        payload.Description.Should().Be(description);
        payload.ImageUrl.Should().Be(ValidImageUrl);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"{ApiRoutes.Species.BaseRoute}/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.Species
            .AsNoTracking()
            .FirstOrDefaultAsync(species => species.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.ScientificName.Should().Be(scientificName.ToSanitized());
        persisted.NormalizedScientificName.Should().Be(scientificName.ToNormalized());
        persisted.CommonName.Should().Be(commonName.ToSanitized());
        persisted.NormalizedCommonName.Should().Be(commonName.ToNormalized());
        persisted.Description.Should().Be(description);
        persisted.ImageUrl.Should().Be(ValidImageUrl);
    }
}
