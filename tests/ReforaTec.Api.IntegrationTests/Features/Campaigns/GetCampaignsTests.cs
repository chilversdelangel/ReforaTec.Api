using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Campaigns.GetCampaigns;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Campaigns;

public class GetCampaignsTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetCampaigns_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task GetCampaigns_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.BaseRoute);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetCampaigns_WhenCampaignsExist_ShouldReturnTenantCampaignsOrderedByStartDateDescending(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var olderCampaign = await CreateCampaignAsync(
            tenantId: actor.TenantId,
            period: new Period { StartDate = today.AddDays(-60), EndDate = today.AddDays(-30) });
        var newerCampaign = await CreateCampaignAsync(
            tenantId: actor.TenantId,
            period: new Period { StartDate = today.AddDays(-5), EndDate = today.AddDays(30) });

        var foreignCampaign = await CreateCampaignAsync(tenantId: TestTenants.Foreign);

        Authenticate(actor);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<Response>>();
        payload.Should().NotBeNull();
        payload.Should().NotContain(item => item.Id == foreignCampaign.Id);
        payload.Select(item => item.Id).Should().ContainInOrder(newerCampaign.Id, olderCampaign.Id);
    }
}
