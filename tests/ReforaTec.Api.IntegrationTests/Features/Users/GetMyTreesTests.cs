using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Features.Users.GetMyTrees;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class GetMyTreesTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetMyTrees_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyTrees_WhenUserBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        AuthenticateWithClaims(userId: user.Id, tenantId: TestTenants.Foreign);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(user.Id));
    }

    // --- 2. Validation ---
    // Validation: N/A — no request body or query parameters

    // --- 3. Business Errors ---
    [Fact]
    public async Task GetMyTrees_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const int nonExistentUserId = 99999;
        AuthenticateWithClaims(userId: nonExistentUserId);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(nonExistentUserId));
    }

    [Fact]
    public async Task GetMyTrees_WhenUserIsSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var deletedUser = await CreateUserAsync(isDeleted: true);
        Authenticate(deletedUser);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(deletedUser.Id));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task GetMyTrees_WhenUserHasNoAssignedTrees_ShouldReturnEmptyList()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().NotBeNull();
        result.Trees.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyTrees_WhenUserHasAssignedTree_ShouldReturnOkWithTreesAndVirtueName()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var campaign = await CreateCampaignAsync();
        var expectedVirtue = await QueryDbAsync(context => context.Virtues.FirstAsync());
        var tree = await CreateTreeAsync(virtueId: expectedVirtue.Id);

        await AssignTreeToUserAsync(tree.Id, user.Id, campaign.Id);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().NotBeNull();
        result.Trees.Should().HaveCount(1);

        var assignedTree = result.Trees[0];
        assignedTree.Id.Should().Be(tree.Id);
        assignedTree.Folio.Should().Be(tree.Folio);
        assignedTree.SpeciesId.Should().Be(tree.SpeciesId);
        assignedTree.VirtueName.Should().Be(expectedVirtue.Name);
    }

    [Fact]
    public async Task GetMyTrees_WhenAssignmentHasEnded_ShouldNotIncludeInactiveTrees()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var campaign = await CreateCampaignAsync();
        var tree = await CreateTreeAsync();
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var lastWeek = yesterday.AddDays(-7);
        await AssignTreeToUserAsync(tree.Id, user.Id, campaign.Id, startDate: lastWeek, endDate: yesterday);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.MyTrees);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().NotBeNull();
        result.Trees.Should().BeEmpty();
    }
}
