using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.CampaignEnrollments.EvaluateCampaignEnrollment;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.CampaignEnrollments;

public class EvaluateCampaignEnrollmentTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task EvaluateCampaignEnrollment_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(IsApproved: true);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.CampaignEnrollments.ById(1), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task EvaluateCampaignEnrollment_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(IsApproved: true);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.CampaignEnrollments.ById(1), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Validation ---
    [Fact]
    public async Task EvaluateCampaignEnrollment_WhenEvaluationNotesExceedMaxLength_ShouldReturnValidationProblem()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var request = new Request(
            IsApproved: false,
            EvaluationNotes: new string('a', 501));

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.CampaignEnrollments.ById(1), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.EvaluationNotes));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task EvaluateCampaignEnrollment_WhenEnrollmentDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        const int nonExistentEnrollmentId = 99999;
        var request = new Request(IsApproved: true);

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CampaignEnrollments.ById(nonExistentEnrollmentId),
            request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.EnrollmentNotFound(nonExistentEnrollmentId));
    }

    [Fact]
    public async Task EvaluateCampaignEnrollment_WhenEnrollmentBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        var foreignStudent = await CreateUserAsync(tenantId: TestTenants.Foreign, role: UserRole.Student);
        var foreignCampaign = await CreateCampaignAsync(tenantId: TestTenants.Foreign);
        var foreignEnrollment = await EnrollUserInCampaignAsync(
            tenantId: TestTenants.Foreign,
            campaignId: foreignCampaign.Id,
            userId: foreignStudent.Id);

        Authenticate(coordinator);
        var request = new Request(IsApproved: true);

        // Act
        var response = await Client.PatchAsJsonAsync(
            ApiRoutes.CampaignEnrollments.ById(foreignEnrollment.Id),
            request);

        // Assert
        await response.ShouldBeProblemAsync(
            HttpStatusCode.NotFound,
            Errors.EnrollmentNotFound(foreignEnrollment.Id));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator, true, "Cumplió con todos los riegos.")]
    [InlineData(UserRole.SystemAdmin, false, "Árbol descuidado durante el periodo.")]
    public async Task EvaluateCampaignEnrollment_WhenValid_ShouldPersistDecisionAndReturnOk(
        UserRole actorRole,
        bool isApproved,
        string notes)
    {
        // Arrange
        var actor = await CreateUserAsync(role: actorRole);
        var student = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        var campaign = await CreateCampaignAsync(tenantId: actor.TenantId);
        var enrollment = await EnrollUserInCampaignAsync(
            tenantId: actor.TenantId,
            campaignId: campaign.Id,
            userId: student.Id);

        Authenticate(actor);
        var request = new Request(
            IsApproved: isApproved,
            EvaluationNotes: $"  {notes}  ");

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.CampaignEnrollments.ById(enrollment.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().Be(enrollment.Id);
        payload.CampaignId.Should().Be(campaign.Id);
        payload.UserId.Should().Be(student.Id);
        payload.IsApproved.Should().Be(isApproved);
        payload.EvaluationNotes.Should().Be(notes);
        payload.EvaluatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        var persisted = await QueryDbAsync(context => context.CampaignEnrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == enrollment.Id));

        persisted.Should().NotBeNull();
        persisted.IsApproved.Should().Be(isApproved);
        persisted.EvaluationNotes.Should().Be(notes);
    }
}
