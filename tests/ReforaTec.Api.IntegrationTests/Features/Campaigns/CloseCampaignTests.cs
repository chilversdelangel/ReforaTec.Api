using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Campaigns.CloseCampaign;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Campaigns;

public class CloseCampaignTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CloseCampaign_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var campaign = await CreateCampaignAsync();

        // Act
        var response = await Client.PostAsync(ApiRoutes.Campaigns.Close(campaign.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task CloseCampaign_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        var campaign = await CreateCampaignAsync(tenantId: user.TenantId);
        Authenticate(user);

        // Act
        var response = await Client.PostAsync(ApiRoutes.Campaigns.Close(campaign.Id), null);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Business Errors ---
    [Fact]
    public async Task CloseCampaign_WhenCampaignDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        const int nonExistentCampaignId = 99999;

        // Act
        var response = await Client.PostAsync(ApiRoutes.Campaigns.Close(nonExistentCampaignId), null);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(nonExistentCampaignId));
    }

    [Fact]
    public async Task CloseCampaign_WhenCampaignBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var foreignCampaign = await CreateCampaignAsync(tenantId: TestTenants.Foreign);
        Authenticate(coordinator);

        // Act
        var response = await Client.PostAsync(ApiRoutes.Campaigns.Close(foreignCampaign.Id), null);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(foreignCampaign.Id));
    }

    [Fact]
    public async Task CloseCampaign_WhenAlreadyClosedWithNoActiveAssignments_ShouldReturnConflict()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredCampaign = await CreateCampaignAsync(
            tenantId: coordinator.TenantId,
            period: new Period { StartDate = today.AddDays(-30), EndDate = today.AddDays(-1) });
        Authenticate(coordinator);

        // Act
        var response = await Client.PostAsync(ApiRoutes.Campaigns.Close(expiredCampaign.Id), null);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.AlreadyClosed);
    }

    // --- 3. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CloseCampaign_WhenActiveWithAssignments_ShouldCloseCampaignAndActiveAssignments(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);
        var student1 = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        var student2 = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var campaign = await CreateCampaignAsync(
            tenantId: actor.TenantId,
            period: new Period { StartDate = today.AddDays(-10), EndDate = today.AddDays(20) });

        var tree1 = await CreateTreeAsync(tenantId: actor.TenantId);
        var tree2 = await CreateTreeAsync(tenantId: actor.TenantId);

        var activeAssignment = await AssignTreeToUserAsync(
            tenantId: actor.TenantId,
            campaignId: campaign.Id,
            userId: student1.Id,
            treeId: tree1.Id,
            startDate: today.AddDays(-5));

        var alreadyClosedDate = today.AddDays(-2);
        var closedAssignment = await AssignTreeToUserAsync(
            tenantId: actor.TenantId,
            campaignId: campaign.Id,
            userId: student2.Id,
            treeId: tree2.Id,
            startDate: today.AddDays(-5),
            endDate: alreadyClosedDate);

        Authenticate(actor);

        // Act
        var response = await Client.PostAsync(ApiRoutes.Campaigns.Close(campaign.Id), null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.CampaignId.Should().Be(campaign.Id);
        payload.CampaignCode.Should().Be(campaign.Code);
        payload.Period.EndDate.Should().Be(today);
        payload.ClosedAssignmentsCount.Should().Be(1);

        var persistedCampaign = await QueryDbAsync(context => context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == campaign.Id));
        persistedCampaign!.Period.EndDate.Should().Be(today);

        var persistedActive = await QueryDbAsync(context => context.CareAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == activeAssignment.Id));
        persistedActive!.EndDate.Should().Be(today);

        var persistedClosed = await QueryDbAsync(context => context.CareAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == closedAssignment.Id));
        persistedClosed!.EndDate.Should().Be(alreadyClosedDate);
    }
}
