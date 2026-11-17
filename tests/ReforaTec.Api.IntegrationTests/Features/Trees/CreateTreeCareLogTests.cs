using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Features.Trees.CreateTreeCareLog;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public class CreateTreeCareLogTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateTreeCareLog_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var tree = await CreateTreeAsync();
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var request = new Request(1, now, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTreeCareLog_WhenTreeBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var foreignTree = await CreateTreeAsync(tenantId: TestTenants.Foreign);
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var request = new Request(1, now, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(foreignTree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(foreignTree.Id));
    }

    [Fact]
    public async Task CreateTreeCareLog_WhenUserIsNotAssignedToTree_ShouldReturnForbidden()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var tree = await CreateTreeAsync();
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var request = new Request(1, now, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.UserNotAssignedToTree);
    }

    [Fact]
    public async Task CreateTreeCareLog_WhenUserAssignmentHasEnded_ShouldReturnForbidden()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var campaign = await CreateCampaignAsync();
        var tree = await CreateTreeAsync();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        await AssignTreeToUserAsync(tree.Id, user.Id, campaign.Id, startDate: yesterday.AddDays(-7), endDate: yesterday);

        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var request = new Request(1, now, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.UserNotAssignedToTree);
    }

    // --- 2. Validation ---
    [Fact]
    public async Task CreateTreeCareLog_WhenNotesExceedMaxLength_ShouldReturnBadRequest()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var tree = await CreateTreeAsync();
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var tooLongNotes = new string('a', 501);
        var request = new Request(1, now, now, tooLongNotes);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Notes));
    }

    [Fact]
    public async Task CreateTreeCareLog_WhenPerformedAtIsInFuture_ShouldReturnBadRequest()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var tree = await CreateTreeAsync();
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var futurePerformedAt = DateTimeOffset.UtcNow.AddHours(1);
        var request = new Request(1, futurePerformedAt, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.PerformedAt));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateTreeCareLog_WhenTreeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        const int nonExistentTreeId = 999_999;
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var request = new Request(1, now, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(nonExistentTreeId), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(nonExistentTreeId));
    }

    [Fact]
    public async Task CreateTreeCareLog_WhenCareTypeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var campaign = await CreateCampaignAsync();
        var tree = await CreateTreeAsync();
        await AssignTreeToUserAsync(tree.Id, user.Id, campaign.Id);

        const int nonExistentCareTypeId = 999_999;
        var now = DateTimeOffset.UtcNow.AddMinutes(-5);
        var request = new Request(nonExistentCareTypeId, now, now, "Riego");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CareTypeNotFound(nonExistentCareTypeId));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task CreateTreeCareLog_WhenRequestIsValid_ShouldReturnCreatedAndPersistInDatabase()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var campaign = await CreateCampaignAsync();
        var tree = await CreateTreeAsync();
        await AssignTreeToUserAsync(tree.Id, user.Id, campaign.Id);

        var careType = await QueryDbAsync(context => context.CareTypes.OrderBy(type => type.Id).FirstAsync());
        var performedAt = DateTimeOffset.UtcNow.AddHours(-3);
        var deviceCapturedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        const string notes = "Riego abundante en la base del árbol.";

        var request = new Request(careType.Id, performedAt, deviceCapturedAt, notes);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Trees.CareLogs(tree.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().NotBeNull();
        result.TreeId.Should().Be(tree.Id);
        result.CareTypeId.Should().Be(careType.Id);
        result.PerformedAt.Should().BeCloseTo(performedAt.UtcDateTime, TimeSpan.FromMilliseconds(1));
        result.DeviceCapturedAt.Should().BeCloseTo(deviceCapturedAt.UtcDateTime, TimeSpan.FromMilliseconds(1));
        result.Notes.Should().Be(notes);

        var persistedCareLog = await QueryDbAsync(context =>
            context.CareLogs.FirstOrDefaultAsync(careLog => careLog.Id == result.Id));

        persistedCareLog.Should().NotBeNull();
        persistedCareLog.TenantId.Should().Be(user.TenantId);
        persistedCareLog.UserId.Should().Be(user.Id);
        persistedCareLog.TreeId.Should().Be(tree.Id);
        persistedCareLog.CareTypeId.Should().Be(careType.Id);
        persistedCareLog.Notes.Should().Be(notes);
    }
}
