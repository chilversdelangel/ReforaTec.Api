using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Trees.CreateTreeInspection;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public sealed class CreateTreeInspectionTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateTreeInspection_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var tree = await CreateTreeAsync();
        var request = new Request(TreeHealthState.Healthy, null, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTreeInspection_WhenUserIsStudent_ShouldReturnForbidden()
    {
        // Arrange
        var student = await CreateUserAsync(role: UserRole.Student);
        Authenticate(student);
        var tree = await CreateTreeAsync();
        var request = new Request(TreeHealthState.Healthy, null, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.InsufficientPermissions);
    }

    [Fact]
    public async Task CreateTreeInspection_WhenTreeBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var foreignTree = await CreateTreeAsync(tenantId: TestTenants.Foreign);
        var request = new Request(TreeHealthState.Healthy, null, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(foreignTree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(foreignTree.Id));
    }

    // --- 2. Validation ---
    [Fact]
    public async Task CreateTreeInspection_WhenNotesExceedMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var tree = await CreateTreeAsync();
        var tooLongNotes = new string('a', 501);
        var request = new Request(TreeHealthState.NeedsAttention, tooLongNotes, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Notes));
    }

    [Fact]
    public async Task CreateTreeInspection_WhenDeviceCapturedAtIsInFuture_ShouldReturnBadRequest()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var tree = await CreateTreeAsync();
        var request = new Request(TreeHealthState.Healthy, null, DateTimeOffset.UtcNow.AddHours(1));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.DeviceCapturedAt));
    }

    [Fact]
    public async Task CreateTreeInspection_WhenHealthStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var tree = await CreateTreeAsync();
        var request = new Request((TreeHealthState)999, null, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.HealthState));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateTreeInspection_WhenTreeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        const int nonExistentTreeId = 999_999;
        var request = new Request(TreeHealthState.Healthy, null, DateTimeOffset.UtcNow);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(nonExistentTreeId), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(nonExistentTreeId));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CreateTreeInspection_WithValidRequest_ShouldCreateInspection(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);
        var tree = await CreateTreeAsync();
        const string notes = "Hojas amarillas.";
        var request = new Request(
            TreeHealthState.NeedsAttention,
            notes,
            DateTimeOffset.UtcNow.AddMinutes(-2));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().BeGreaterThan(0);
        payload.TreeId.Should().Be(tree.Id);
        payload.HealthState.Should().Be(nameof(TreeHealthState.NeedsAttention));
        payload.Notes.Should().Be(notes);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var inspectionCount = await appDbContext.Inspections.CountAsync(inspection => inspection.TreeId == tree.Id);
        inspectionCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateTreeInspection_WithoutNotes_ShouldCreateInspection()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);
        var tree = await CreateTreeAsync();
        var request = new Request(
            TreeHealthState.Healthy,
            null, 
            DateTimeOffset.UtcNow.AddMinutes(-1));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.Inspections(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Notes.Should().BeNull();
    }
}
