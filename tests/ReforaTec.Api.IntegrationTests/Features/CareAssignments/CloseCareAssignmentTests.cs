using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.CareAssignments.CloseCareAssignment;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.CareAssignments;

public class CloseCareAssignmentTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task CloseCareAssignment_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var student = await CreateUserAsync(role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: student.TenantId);
        var tree = await CreateTreeAsync(tenantId: student.TenantId);
        var assignment = await AssignTreeToUserAsync(
            tenantId: student.TenantId,
            campaignId: campaign.Id,
            userId: student.Id,
            treeId: tree.Id);
        var request = new Request(EndDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.CareAssignments.ById(assignment.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task CloseCareAssignment_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        var student = await CreateUserAsync(tenantId: user.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: user.TenantId);
        var tree = await CreateTreeAsync(tenantId: user.TenantId);
        var assignment = await AssignTreeToUserAsync(
            tenantId: user.TenantId,
            campaignId: campaign.Id,
            userId: student.Id,
            treeId: tree.Id);
        Authenticate(user);

        var request = new Request(EndDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.CareAssignments.ById(assignment.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Business Errors ---
    [Fact]
    public async Task CloseCareAssignment_WhenAssignmentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        const int nonExistentAssignmentId = 99999;
        var request = new Request(EndDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CareAssignments.ById(nonExistentAssignmentId),
            request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.AssignmentNotFound(nonExistentAssignmentId));
    }

    [Fact]
    public async Task CloseCareAssignment_WhenAssignmentBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var foreignStudent = await CreateUserAsync(tenantId: TestTenants.Foreign, role: UserRole.Student);
        var foreignCampaign = await CreateCampaignAsync(tenantId: TestTenants.Foreign);
        var foreignTree = await CreateTreeAsync(tenantId: TestTenants.Foreign);
        var foreignAssignment = await AssignTreeToUserAsync(
            tenantId: TestTenants.Foreign,
            campaignId: foreignCampaign.Id,
            userId: foreignStudent.Id,
            treeId: foreignTree.Id);

        Authenticate(coordinator);
        var request = new Request(EndDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CareAssignments.ById(foreignAssignment.Id),
            request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.AssignmentNotFound(foreignAssignment.Id));
    }

    [Fact]
    public async Task CloseCareAssignment_WhenAlreadyClosed_ShouldReturnConflict()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var student = await CreateUserAsync(tenantId: coordinator.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: coordinator.TenantId);
        var tree = await CreateTreeAsync(tenantId: coordinator.TenantId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var closedAssignment = await AssignTreeToUserAsync(
            tenantId: coordinator.TenantId,
            campaignId: campaign.Id,
            userId: student.Id,
            treeId: tree.Id,
            startDate: today.AddDays(-10),
            endDate: today.AddDays(-1));

        Authenticate(coordinator);
        var request = new Request(EndDate: today);

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CareAssignments.ById(closedAssignment.Id),
            request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.AlreadyClosed);
    }

    [Fact]
    public async Task CloseCareAssignment_WhenEndDateIsBeforeStartDate_ShouldReturnBadRequest()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var student = await CreateUserAsync(tenantId: coordinator.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: coordinator.TenantId);
        var tree = await CreateTreeAsync(tenantId: coordinator.TenantId);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignment = await AssignTreeToUserAsync(
            tenantId: coordinator.TenantId,
            campaignId: campaign.Id,
            userId: student.Id,
            treeId: tree.Id,
            startDate: startDate);

        Authenticate(coordinator);
        var request = new Request(EndDate: startDate.AddDays(-1));

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CareAssignments.ById(assignment.Id),
            request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, Errors.EndDateBeforeStartDate);
    }

    // --- 3. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task CloseCareAssignment_WhenValid_ShouldPersistEndDateAndReturnOk(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);
        var student = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: actor.TenantId);
        var tree = await CreateTreeAsync(tenantId: actor.TenantId);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignment = await AssignTreeToUserAsync(
            tenantId: actor.TenantId,
            campaignId: campaign.Id,
            userId: student.Id,
            treeId: tree.Id,
            startDate: startDate);

        Authenticate(actor);
        var request = new Request(EndDate: endDate);

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CareAssignments.ById(assignment.Id),
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().Be(assignment.Id);
        payload.CampaignId.Should().Be(campaign.Id);
        payload.UserId.Should().Be(student.Id);
        payload.TreeId.Should().Be(tree.Id);
        payload.StartDate.Should().Be(startDate);
        payload.EndDate.Should().Be(endDate);

        var persisted = await QueryDbAsync(context => context.CareAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == assignment.Id));

        persisted.Should().NotBeNull();
        persisted.EndDate.Should().Be(endDate);
    }
}
