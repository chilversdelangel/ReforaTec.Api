using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Campaigns.GetCampaignEnrollments;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Campaigns;

public class GetCampaignEnrollmentsTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetCampaignEnrollments_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.Enrollments(1));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task GetCampaignEnrollments_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.Enrollments(1));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Business Errors ---
    [Fact]
    public async Task GetCampaignEnrollments_WhenCampaignDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        const int nonExistentCampaignId = 99999;

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.Enrollments(nonExistentCampaignId));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(nonExistentCampaignId));
    }

    [Fact]
    public async Task GetCampaignEnrollments_WhenCampaignBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var foreignCampaign = await CreateCampaignAsync(tenantId: TestTenants.Foreign);
        Authenticate(coordinator);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.Enrollments(foreignCampaign.Id));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(foreignCampaign.Id));
    }

    // --- 3. Happy Path ---
    [Fact]
    public async Task GetCampaignEnrollments_WhenNoEnrollmentsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var campaign = await CreateCampaignAsync(tenantId: coordinator.TenantId);
        Authenticate(coordinator);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.Enrollments(campaign.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<Response>>();
        payload.Should().NotBeNull();
        payload.Should().BeEmpty();
    }

    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetCampaignEnrollments_WhenEnrollmentsExist_ShouldReturnAllEnrollmentsForCampaign(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);
        var campaign = await CreateCampaignAsync(tenantId: actor.TenantId);
        var student1 = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        var student2 = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);

        var enrollment1 = await EnrollUserInCampaignAsync(tenantId: actor.TenantId, campaignId: campaign.Id, userId: student1.Id);
        var enrollment2 = await EnrollUserInCampaignAsync(
            tenantId: actor.TenantId,
            campaignId: campaign.Id,
            userId: student2.Id,
            isApproved: true,
            evaluationNotes: "Crédito acreditado con excelencia");

        Authenticate(actor);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Campaigns.Enrollments(campaign.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<List<Response>>();
        payload.Should().NotBeNull();
        payload.Should().HaveCount(2);
        payload.Select(item => item.Id).Should().ContainInOrder(enrollment1.Id, enrollment2.Id);

        var first = payload.Single(item => item.Id == enrollment1.Id);
        first.User.Id.Should().Be(student1.Id);
        first.User.GivenName.Should().Be(student1.GivenName);
        first.User.FamilyName.Should().Be(student1.FamilyName);
        first.User.SecondFamilyName.Should().Be(student1.SecondFamilyName);
        first.User.DisplayName.Should().Be(student1.DisplayName);
        first.User.Email.Should().Be(student1.Email);
        first.User.ControlNumber.Should().Be(student1.ControlNumber);
        first.IsApproved.Should().BeNull();
        first.EvaluationNotes.Should().BeNull();
        first.EnrolledAt.Should().BeCloseTo(enrollment1.CreatedAt, TimeSpan.FromSeconds(1));
        first.EvaluatedAt.Should().BeNull();

        var second = payload.Single(item => item.Id == enrollment2.Id);
        second.User.Id.Should().Be(student2.Id);
        second.User.GivenName.Should().Be(student2.GivenName);
        second.User.FamilyName.Should().Be(student2.FamilyName);
        second.User.SecondFamilyName.Should().Be(student2.SecondFamilyName);
        second.User.DisplayName.Should().Be(student2.DisplayName);
        second.User.Email.Should().Be(student2.Email);
        second.User.ControlNumber.Should().Be(student2.ControlNumber);
        second.IsApproved.Should().BeTrue();
        second.EvaluationNotes.Should().Be("Crédito acreditado con excelencia");
        second.EnrolledAt.Should().BeCloseTo(enrollment2.CreatedAt, TimeSpan.FromSeconds(1));
        second.EvaluatedAt.Should().NotBeNull();
        second.EvaluatedAt!.Value.Should().BeCloseTo(enrollment2.ModifiedAt, TimeSpan.FromSeconds(1));
    }
}
