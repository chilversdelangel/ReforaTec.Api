using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Virtues.CreateVirtue;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Virtues;

public class CreateVirtueTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateVirtue_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(Name: "Integridad");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Virtues.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    public async Task CreateVirtue_WhenAuthenticatedAsNonAdmin_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(Name: "Integridad");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Virtues.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("NombreDeVirtudDemasiadoLargo")]
    public async Task CreateVirtue_WhenNameIsInvalid_ShouldReturnValidationProblem(string invalidName)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var request = new Request(Name: invalidName);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Virtues.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Name));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateVirtue_WhenNameAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        var virtueName = $"Virtud-{Guid.NewGuid().ToString("N")[..6]}";
        await CreateVirtueAsync(name: virtueName);
        Authenticate(admin);

        var upperName = virtueName.ToUpperInvariant();
        var request = new Request(Name: $"  {upperName}  ");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Virtues.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.DuplicateName(upperName));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task CreateVirtue_WhenValid_ShouldPersistAndReturnCreated()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var virtueName = $"Templanza-{Guid.NewGuid().ToString("N")[..4]}";
        var request = new Request(Name: $"  {virtueName}  ");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Virtues.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().BeGreaterThan(0);
        payload.Name.Should().Be(virtueName);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"{ApiRoutes.Virtues.BaseRoute}/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.Virtues
            .AsNoTracking()
            .FirstOrDefaultAsync(virtue => virtue.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.Name.Should().Be(virtueName);
        persisted.NormalizedName.Should().Be(virtueName.ToNormalized());
    }
}
