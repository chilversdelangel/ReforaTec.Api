using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Users.GetMyCampaignEnrollment;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class GetMyCampaignEnrollmentTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetMyCampaignEnrollment_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.CampaignEnrollment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- 2. Happy Path ---
    [Fact]
    public async Task GetMyCampaignEnrollment_WhenNotEnrolled_ShouldReturnFalse()
    {
        // Arrange
        var student = await CreateUserAsync(role: UserRole.Student);
        Authenticate(student);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.CampaignEnrollment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.IsEnrolled.Should().BeFalse();
    }

    [Fact]
    public async Task GetMyCampaignEnrollment_WhenEnrolledOnlyInExpiredCampaign_ShouldReturnFalse()
    {
        // Arrange
        var student = await CreateUserAsync(role: UserRole.Student);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredCampaign = await CreateCampaignAsync(
            tenantId: student.TenantId,
            period: new Period { StartDate = today.AddDays(-30), EndDate = today.AddDays(-1) });
        await EnrollUserInCampaignAsync(tenantId: student.TenantId, campaignId: expiredCampaign.Id, userId: student.Id);

        Authenticate(student);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.CampaignEnrollment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.IsEnrolled.Should().BeFalse();
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetMyCampaignEnrollment_WhenEnrolledInActiveCampaign_ShouldReturnTrue(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeCampaign = await CreateCampaignAsync(
            tenantId: user.TenantId,
            period: new Period { StartDate = today.AddDays(-5), EndDate = today.AddDays(30) });
        await EnrollUserInCampaignAsync(tenantId: user.TenantId, campaignId: activeCampaign.Id, userId: user.Id);

        Authenticate(user);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.CampaignEnrollment);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.IsEnrolled.Should().BeTrue();
    }
}
