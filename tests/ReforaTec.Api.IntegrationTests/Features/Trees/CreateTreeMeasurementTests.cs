using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Trees.CreateTreeMeasurement;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public sealed class CreateTreeMeasurementTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateTreeMeasurement_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var tree = await CreateTreeAsync();
        var request = new Request(150.0m, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTreeMeasurement_WhenUserIsStudent_ShouldReturnForbidden()
    {
        // Arrange
        var student = await CreateUserAsync(role: UserRole.Student);
        Authenticate(student);
        var tree = await CreateTreeAsync();
        var request = new Request(150.0m, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(tree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.InsufficientPermissions);
    }

    [Fact]
    public async Task CreateTreeMeasurement_WhenTreeBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var foreignTree = await CreateTreeAsync(tenantId: TestTenants.Foreign);
        var request = new Request(150.0m, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(foreignTree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(foreignTree.Id));
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateTreeMeasurement_WhenHeightIsNotPositive_ShouldReturnBadRequest(decimal invalidHeight)
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var tree = await CreateTreeAsync();
        var request = new Request(invalidHeight, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.HeightCentimeters));
    }

    [Fact]
    public async Task CreateTreeMeasurement_WhenDeviceCapturedAtIsInFuture_ShouldReturnBadRequest()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var tree = await CreateTreeAsync();
        var request = new Request(150.0m, DateTimeOffset.UtcNow.AddHours(1));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.DeviceCapturedAt));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateTreeMeasurement_WhenTreeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        const int nonExistentTreeId = 999_999;
        var request = new Request(150.0m, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(nonExistentTreeId), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(nonExistentTreeId));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CreateTreeMeasurement_WithValidRequest_ShouldCreateMeasurement(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);
        var tree = await CreateTreeAsync();
        const decimal heightCentimeters = 162.5m;
        var request = new Request(heightCentimeters, DateTimeOffset.UtcNow.AddMinutes(-2));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Measurements(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().BeGreaterThan(0);
        payload.TreeId.Should().Be(tree.Id);
        payload.HeightCentimeters.Should().Be(heightCentimeters);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var measurementCount = await appDbContext.Measurements.CountAsync(measurement => measurement.TreeId == tree.Id);
        measurementCount.Should().Be(1);
    }
}
