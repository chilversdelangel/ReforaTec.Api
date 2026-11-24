using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Campaigns.GetCampaignById;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Campaigns;

public class GetCampaignByIdTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetCampaignById_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.ById(1));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task GetCampaignById_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.ById(1));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Business Errors ---
    [Fact]
    public async Task GetCampaignById_WhenCampaignDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        const int nonExistentCampaignId = 99999;

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.ById(nonExistentCampaignId));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(nonExistentCampaignId));
    }

    [Fact]
    public async Task GetCampaignById_WhenCampaignBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var foreignCampaign = await CreateCampaignAsync(tenantId: TestTenants.Foreign);
        Authenticate(coordinator);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.ById(foreignCampaign.Id));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(foreignCampaign.Id));
    }

    // --- 3. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetCampaignById_WhenCampaignExists_ShouldReturnCampaign(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);
        var campaign = await CreateCampaignAsync(tenantId: actor.TenantId);
        Authenticate(actor);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.ById(campaign.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().Be(campaign.Id);
        payload.Code.Should().Be(campaign.Code);
        payload.Period.StartDate.Should().Be(campaign.Period.StartDate);
        payload.Period.EndDate.Should().Be(campaign.Period.EndDate);
    }
}
