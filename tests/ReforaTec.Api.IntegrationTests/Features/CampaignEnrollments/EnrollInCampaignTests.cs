using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.CampaignEnrollments.EnrollInCampaign;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.CampaignEnrollments;

public class EnrollInCampaignTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task EnrollInCampaign_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(CampaignCode: "ITCM-2026-A");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task EnrollInCampaign_WhenAuthenticatedWithAnyRole_ShouldReturnCreated(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        var campaign = await CreateCampaignAsync(
            tenantId: user.TenantId,
            code: "ITCM-2026-A",
            period: new Period
            {
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
            });
        Authenticate(user);

        var request = new Request(CampaignCode: campaign.Code);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task EnrollInCampaign_WhenCampaignBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        const string code = "FOREIGN-2026-A";
        var student = await CreateUserAsync();
        Authenticate(student);

        await CreateCampaignAsync(tenantId: TestTenants.Foreign, code: code);

        var request = new Request(CampaignCode: code);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(code));
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901")]
    public async Task EnrollInCampaign_WhenCampaignCodeIsEmptyOrTooLong_ShouldReturnBadRequest(string campaignCode)
    {
        // Arrange
        var student = await CreateUserAsync();
        Authenticate(student);

        var request = new Request(CampaignCode: campaignCode);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.CampaignCode));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task EnrollInCampaign_WhenCampaignDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const string nonExistentCode = "MISSING-2026-A";
        var student = await CreateUserAsync();
        Authenticate(student);

        var request = new Request(CampaignCode: nonExistentCode);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.CampaignNotFound(nonExistentCode));
    }

    [Theory]
    [InlineData(-60, -1)]
    [InlineData(1, 60)]
    public async Task EnrollInCampaign_WhenCampaignIsNotActive_ShouldReturnConflict(int startOffsetDays, int endOffsetDays)
    {
        // Arrange
        const string code = "INACTIVE-2026-A";
        var student = await CreateUserAsync();
        Authenticate(student);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await CreateCampaignAsync(
            tenantId: student.TenantId,
            code: code,
            period: new Period
            {
                StartDate = today.AddDays(startOffsetDays),
                EndDate = today.AddDays(endOffsetDays)
            });

        var request = new Request(CampaignCode: code);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.CampaignNotActive);
    }

    [Fact]
    public async Task EnrollInCampaign_WhenAlreadyEnrolled_ShouldReturnConflict()
    {
        // Arrange
        const string code = "ITCM-2026-A";
        var student = await CreateUserAsync();
        Authenticate(student);

        var campaign = await CreateCampaignAsync(tenantId: student.TenantId, code: code);
        await EnrollUserInCampaignAsync(
            campaignId: campaign.Id,
            userId: student.Id,
            tenantId: student.TenantId);

        var request = new Request(CampaignCode: code);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.AlreadyEnrolled);
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task EnrollInCampaign_WhenValidActiveCampaign_ShouldPersistEnrollmentAndReturnCreated()
    {
        // Arrange
        const string code = "ITCM-2026-A";
        var student = await CreateUserAsync();
        Authenticate(student);

        var campaign = await CreateCampaignAsync(tenantId: student.TenantId, code: code);
        var request = new Request(CampaignCode: $" {code.ToLowerInvariant()} ");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CampaignEnrollments.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.CampaignId.Should().Be(campaign.Id);
        payload.CampaignCode.Should().Be(campaign.Code);
        payload.Period.StartDate.Should().Be(campaign.Period.StartDate);
        payload.Period.EndDate.Should().Be(campaign.Period.EndDate);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"{ApiRoutes.CampaignEnrollments.BaseRoute}/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.CampaignEnrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(enrollment => enrollment.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.TenantId.Should().Be(student.TenantId);
        persisted.CampaignId.Should().Be(campaign.Id);
        persisted.UserId.Should().Be(student.Id);
        persisted.IsApproved.Should().BeNull();
    }
}
