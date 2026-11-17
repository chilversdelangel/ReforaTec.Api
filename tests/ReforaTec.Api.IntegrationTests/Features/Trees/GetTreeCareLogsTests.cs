using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Features.Trees.GetTreeCareLogs;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public class GetTreeCareLogsTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetTreeCareLogs_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var tree = await CreateTreeAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.CareLogs(tree.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTreeCareLogs_WhenTreeBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var foreignTree = await CreateTreeAsync(tenantId: TestTenants.Foreign);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.CareLogs(foreignTree.Id));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(foreignTree.Id));
    }

    // --- 2. Validation ---
    // Validation: N/A — route constraint :int handles non-integer paths

    // --- 3. Business Errors ---
    [Fact]
    public async Task GetTreeCareLogs_WhenTreeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);
        const int nonExistentTreeId = 999_999;

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.CareLogs(nonExistentTreeId));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(nonExistentTreeId));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task GetTreeCareLogs_WhenTreeHasNoCareLogs_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var tree = await CreateTreeAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.CareLogs(tree.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().NotBeNull();
        result.CareLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTreeCareLogs_WhenTreeHasCareLogs_ShouldReturnOkOrderedByDeviceCapturedAtDescending()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var tree = await CreateTreeAsync();
        var careType = await QueryDbAsync(context => context.CareTypes.OrderBy(type => type.Id).FirstAsync());

        var olderCapturedAt = DateTime.UtcNow.AddDays(-3);
        var olderPerformedAt = DateTime.UtcNow.AddDays(-4);
        const string olderNotes = "Riego inicial.";

        var newerCapturedAt = DateTime.UtcNow.AddHours(-2);
        var newerPerformedAt = DateTime.UtcNow.AddHours(-5);
        const string newerNotes = "Poda ligera de ramas secas.";

        var olderLog = await CreateCareLogAsync(
            tree.Id,
            user.Id,
            careTypeId: careType.Id,
            performedAt: olderPerformedAt,
            deviceCapturedAt: olderCapturedAt,
            notes: olderNotes);

        var newerLog = await CreateCareLogAsync(
            tree.Id,
            user.Id,
            careTypeId: careType.Id,
            performedAt: newerPerformedAt,
            deviceCapturedAt: newerCapturedAt,
            notes: newerNotes);

        var expectedNewerDto = new CareLogDto(
            Id: newerLog.Id,
            TreeId: tree.Id,
            CareTypeId: careType.Id,
            CareTypeName: careType.Name,
            UserId: user.Id,
            PerformedAt: newerLog.PerformedAt,
            DeviceCapturedAt: newerLog.DeviceCapturedAt,
            CreatedAt: newerLog.CreatedAt,
            Notes: newerNotes);

        var expectedOlderDto = new CareLogDto(
            Id: olderLog.Id,
            TreeId: tree.Id,
            CareTypeId: careType.Id,
            CareTypeName: careType.Name,
            UserId: user.Id,
            PerformedAt: olderLog.PerformedAt,
            DeviceCapturedAt: olderLog.DeviceCapturedAt,
            CreatedAt: olderLog.CreatedAt,
            Notes: olderNotes);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.CareLogs(tree.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().NotBeNull();
        result.CareLogs.Should().HaveCount(2);

        AssertCareLogMatches(result.CareLogs[0], expectedNewerDto);
        AssertCareLogMatches(result.CareLogs[1], expectedOlderDto);
    }

    private static void AssertCareLogMatches(CareLogDto actual, CareLogDto expected)
    {
        actual.Id.Should().Be(expected.Id);
        actual.TreeId.Should().Be(expected.TreeId);
        actual.CareTypeId.Should().Be(expected.CareTypeId);
        actual.CareTypeName.Should().Be(expected.CareTypeName);
        actual.UserId.Should().Be(expected.UserId);
        actual.Notes.Should().Be(expected.Notes);

        actual.PerformedAt.Should().BeCloseTo(expected.PerformedAt, TimeSpan.FromMilliseconds(1));
        actual.DeviceCapturedAt.Should().BeCloseTo(expected.DeviceCapturedAt, TimeSpan.FromMilliseconds(1));
        actual.CreatedAt.Should().BeCloseTo(expected.CreatedAt, TimeSpan.FromMilliseconds(1));
    }
}
