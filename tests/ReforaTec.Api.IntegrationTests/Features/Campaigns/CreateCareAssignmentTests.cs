using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Campaigns.CreateCareAssignment;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Campaigns;

public class CreateCareAssignmentTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CreateCareAssignment_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(UserId: 1, TreeId: 1, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(1), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task CreateCareAssignment_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(UserId: 1, TreeId: 1, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(1), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateCareAssignment_WhenUserIdIsInvalid_ShouldReturnValidationProblem(int invalidUserId)
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var request = new Request(UserId: invalidUserId, TreeId: 1, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(1), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.UserId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateCareAssignment_WhenTreeIdIsInvalid_ShouldReturnValidationProblem(int invalidTreeId)
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var request = new Request(UserId: 1, TreeId: invalidTreeId, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(1), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.TreeId));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateCareAssignment_WhenCampaignDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        const int nonExistentCampaignId = 99999;
        var request = new Request(UserId: 1, TreeId: 1, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(nonExistentCampaignId), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.CampaignNotFound(nonExistentCampaignId));
    }

    [Fact]
    public async Task CreateCareAssignment_WhenCampaignIsNotActive_ShouldReturnConflict()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiredCampaign = await CreateCampaignAsync(
            tenantId: coordinator.TenantId,
            period: new Period { StartDate = today.AddDays(-30), EndDate = today.AddDays(-1) });
        Authenticate(coordinator);

        var request = new Request(UserId: 1, TreeId: 1, StartDate: today);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(expiredCampaign.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.CampaignNotActive);
    }

    [Fact]
    public async Task CreateCareAssignment_WhenUserIsNotEnrolled_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var student = await CreateUserAsync(tenantId: coordinator.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: coordinator.TenantId);
        Authenticate(coordinator);

        var request = new Request(UserId: student.Id, TreeId: 1, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(campaign.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.EnrollmentNotFound(student.Id, campaign.Id));
    }

    [Fact]
    public async Task CreateCareAssignment_WhenTreeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var student = await CreateUserAsync(tenantId: coordinator.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: coordinator.TenantId);
        await EnrollUserInCampaignAsync(tenantId: coordinator.TenantId, campaignId: campaign.Id, userId: student.Id);

        const int nonExistentTreeId = 99999;
        Authenticate(coordinator);

        var request = new Request(UserId: student.Id, TreeId: nonExistentTreeId, StartDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(campaign.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(nonExistentTreeId));
    }

    [Fact]
    public async Task CreateCareAssignment_WhenActiveAssignmentAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var student = await CreateUserAsync(tenantId: coordinator.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: coordinator.TenantId);
        await EnrollUserInCampaignAsync(tenantId: coordinator.TenantId, campaignId: campaign.Id, userId: student.Id);
        var tree = await CreateTreeAsync(tenantId: coordinator.TenantId);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await AssignTreeToUserAsync(
            tenantId: coordinator.TenantId,
            campaignId: campaign.Id,
            userId: student.Id,
            treeId: tree.Id,
            startDate: startDate);

        Authenticate(coordinator);
        var request = new Request(UserId: student.Id, TreeId: tree.Id, StartDate: startDate);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(campaign.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.AlreadyAssigned);
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CreateCareAssignment_WhenValid_ShouldPersistAndReturnCreated(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);
        var student = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: actor.TenantId);
        await EnrollUserInCampaignAsync(tenantId: actor.TenantId, campaignId: campaign.Id, userId: student.Id);
        var tree = await CreateTreeAsync(tenantId: actor.TenantId);
        Authenticate(actor);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var request = new Request(UserId: student.Id, TreeId: tree.Id, StartDate: startDate);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Campaigns.CareAssignments(campaign.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.CampaignId.Should().Be(campaign.Id);
        payload.UserId.Should().Be(student.Id);
        payload.TreeId.Should().Be(tree.Id);
        payload.StartDate.Should().Be(startDate);
        payload.EndDate.Should().BeNull();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"/v1/care-assignments/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.CareAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(assignment => assignment.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.TenantId.Should().Be(actor.TenantId);
        persisted.CampaignId.Should().Be(campaign.Id);
        persisted.UserId.Should().Be(student.Id);
        persisted.TreeId.Should().Be(tree.Id);
        persisted.StartDate.Should().Be(startDate);
        persisted.EndDate.Should().BeNull();
    }
}
