using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Trees.CreateTree;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public class CreateTreeTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateTree_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(
            Folio: "01",
            SpeciesId: 1,
            VirtueId: 1,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task CreateTree_WhenAuthenticatedWithoutManagementRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(
            Folio: "02",
            SpeciesId: 1,
            VirtueId: 1,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345678901")]
    public async Task CreateTree_WhenFolioIsEmptyOrTooLong_ShouldReturnBadRequest(string folio)
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var request = new Request(
            Folio: folio,
            SpeciesId: 1,
            VirtueId: 1,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Folio));
    }

    [Theory]
    [InlineData(-90.1, -97.8, $"{nameof(Request.Location)}.{nameof(TreeLocationRequest.Latitude)}")]
    [InlineData(90.1, -97.8, $"{nameof(Request.Location)}.{nameof(TreeLocationRequest.Latitude)}")]
    [InlineData(22.25, -180.1, $"{nameof(Request.Location)}.{nameof(TreeLocationRequest.Longitude)}")]
    [InlineData(22.25, 180.1, $"{nameof(Request.Location)}.{nameof(TreeLocationRequest.Longitude)}")]
    public async Task CreateTree_WhenCoordinatesAreOutOfRange_ShouldReturnBadRequest(
        double latitude,
        double longitude,
        string expectedField)
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var request = new Request(
            Folio: "03",
            SpeciesId: 1,
            VirtueId: 1,
            PlantingDate: null,
            Location: new TreeLocationRequest(latitude, longitude));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(expectedField);
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateTree_WhenFolioAlreadyExistsInTenant_ShouldReturnConflict()
    {
        // Arrange
        const string folio = "04";
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var species = await CreateSpeciesAsync();
        var virtue = await CreateVirtueAsync();
        await CreateTreeAsync(
            tenantId: coordinator.TenantId,
            speciesId: species.Id,
            virtueId: virtue.Id,
            folio: folio);

        var request = new Request(
            Folio: $" {folio} ",
            SpeciesId: species.Id,
            VirtueId: virtue.Id,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict,
            Errors.FolioAlreadyExists(folio));
    }

    [Fact]
    public async Task CreateTree_WhenSpeciesDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const int nonExistentSpeciesId = 999999;
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var virtue = await CreateVirtueAsync();
        var request = new Request(
            Folio: "05",
            SpeciesId: nonExistentSpeciesId,
            VirtueId: virtue.Id,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.SpeciesNotFound(nonExistentSpeciesId));
    }

    [Fact]
    public async Task CreateTree_WhenVirtueDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const int nonExistentVirtueId = 999999;
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var species = await CreateSpeciesAsync();
        var request = new Request(
            Folio: "06",
            SpeciesId: species.Id,
            VirtueId: nonExistentVirtueId,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.VirtueNotFound(nonExistentVirtueId));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CreateTree_WhenValidRequest_ShouldPersistTreeAndReturnCreated(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var species = await CreateSpeciesAsync();
        var virtue = await CreateVirtueAsync();
        var plantingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var location = new TreeLocationRequest(22.2515, -97.8342);
        var folio = role == UserRole.Coordinator ? "10" : "11";

        var request = new Request(
            Folio: folio,
            SpeciesId: species.Id,
            VirtueId: virtue.Id,
            PlantingDate: plantingDate,
            Location: location);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Folio.Should().Be(folio);
        payload.SpeciesId.Should().Be(species.Id);
        payload.VirtueId.Should().Be(virtue.Id);
        payload.PlantingDate.Should().Be(plantingDate);
        payload.Location.Should().BeEquivalentTo(location);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"/v1/trees/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.Trees
            .AsNoTracking()
            .FirstOrDefaultAsync(tree => tree.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.TenantId.Should().Be(user.TenantId);
        persisted.Folio.Should().Be(folio);
        persisted.NormalizedFolio.Should().Be(folio.ToNormalized());
        persisted.SpeciesId.Should().Be(species.Id);
        persisted.VirtueId.Should().Be(virtue.Id);
        persisted.PlantingDate.Should().Be(plantingDate);
        persisted.Location.Should().BeEquivalentTo(location);
    }

    [Fact]
    public async Task CreateTree_WhenSameFolioExistsInForeignTenant_ShouldReturnCreated()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var species = await CreateSpeciesAsync();
        var virtue = await CreateVirtueAsync();
        await CreateTreeAsync(
            tenantId: TestTenants.Foreign,
            speciesId: species.Id,
            virtueId: virtue.Id,
            folio: "12");

        var request = new Request(
            Folio: "12",
            SpeciesId: species.Id,
            VirtueId: virtue.Id,
            PlantingDate: null,
            Location: null);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
