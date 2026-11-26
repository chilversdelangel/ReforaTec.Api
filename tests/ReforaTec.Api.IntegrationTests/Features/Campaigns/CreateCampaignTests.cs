using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Campaigns.CreateCampaign;
using ReforaTec.Api.Features.Common.Dtos;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Campaigns;

public class CreateCampaignTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateCampaign_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(
            Code: "ITCM-2026-A",
            Period: new PeriodDto(new DateOnly(2026, 2, 1), new DateOnly(2026, 6, 30)));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task CreateCampaign_WhenAuthenticatedWithoutManagementRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(
            Code: "ITCM-2026-A",
            Period: new PeriodDto(new DateOnly(2026, 2, 1), new DateOnly(2026, 6, 30)));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123456789012345678901")]
    public async Task CreateCampaign_WhenCodeIsEmptyOrTooLong_ShouldReturnBadRequest(string code)
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var request = new Request(
            Code: code,
            Period: new PeriodDto(new DateOnly(2026, 2, 1), new DateOnly(2026, 6, 30)));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Code));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateCampaign_WhenEndDateIsNotAfterStartDate_ShouldReturnBadRequest(int dayOffset)
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var startDate = new DateOnly(2026, 2, 10);
        var request = new Request(
            Code: "ITCM-2026-A",
            Period: new PeriodDto(startDate, startDate.AddDays(dayOffset)));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync($"{nameof(Request.Period)}.{nameof(PeriodDto.EndDate)}");
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateCampaign_WhenCodeAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        const string code = "ITCM-2026-A";
        var duplicateCode = code.ToLowerInvariant();
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        await CreateCampaignAsync(tenantId: coordinator.TenantId, code: code);

        var request = new Request(
            Code: $" {duplicateCode} ",
            Period: new PeriodDto(new DateOnly(2026, 2, 1), new DateOnly(2026, 6, 30)));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict,
            Errors.CodeAlreadyExists(duplicateCode));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CreateCampaign_WhenValidRequest_ShouldPersistCampaignAndReturnCreated(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var code = role == UserRole.Coordinator ? "ITCM-2026-A" : "ITCM-2026-B";
        var period = new PeriodDto(
            new DateOnly(2026, 2, 1), 
            new DateOnly(2026, 6, 30));
        var request = new Request(Code: code, Period: period);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Code.Should().Be(code);
        payload.Period.Should().Be(period);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"/v1/campaigns/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(campaign => campaign.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.TenantId.Should().Be(user.TenantId);
        persisted.Code.Should().Be(code);
        persisted.NormalizedCode.Should().Be(code.ToNormalized());
        persisted.Period.StartDate.Should().Be(period.StartDate);
        persisted.Period.EndDate.Should().Be(period.EndDate!.Value);
    }

    [Fact]
    public async Task CreateCampaign_WhenSameCodeExistsInForeignTenant_ShouldReturnCreated()
    {
        // Arrange
        const string code = "2026-A";
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        await CreateCampaignAsync(tenantId: TestTenants.Foreign, code: code);

        var request = new Request(
            Code: code,
            Period: new PeriodDto(new DateOnly(2026, 2, 1), new DateOnly(2026, 6, 30)));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCampaign_WhenEndDateIsNull_ShouldPersistOpenEndedCampaign()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var code = $"OPEN-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var request = new Request(code, new PeriodDto(startDate, EndDate: null));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Code.Should().Be(code.ToSanitized());
        payload.Period.StartDate.Should().Be(startDate);
        payload.Period.EndDate.Should().BeNull();
    }
}
