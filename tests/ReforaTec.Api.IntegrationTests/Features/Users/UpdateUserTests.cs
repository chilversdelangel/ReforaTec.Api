using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Users.UpdateUser;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class UpdateUserTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task UpdateUser_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(Role: UserRole.Inspector);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(1), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task UpdateUser_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole actorRole)
    {
        // Arrange
        var actor = await CreateUserAsync(role: actorRole);
        var target = await CreateUserAsync(tenantId: actor.TenantId, role: UserRole.Student);
        Authenticate(actor);

        var request = new Request(Role: UserRole.Inspector);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(target.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    [Theory]
    [InlineData(UserRole.Coordinator, UserRole.Student, UserRole.Coordinator)]
    [InlineData(UserRole.Coordinator, UserRole.Student, UserRole.SystemAdmin)]
    [InlineData(UserRole.SystemAdmin, UserRole.Student, UserRole.SystemAdmin)]
    [InlineData(UserRole.Coordinator, UserRole.Coordinator, UserRole.Student)]
    [InlineData(UserRole.Coordinator, UserRole.SystemAdmin, UserRole.Student)]
    [InlineData(UserRole.SystemAdmin, UserRole.SystemAdmin, UserRole.Student)]
    public async Task UpdateUser_WhenHierarchyIsViolated_ShouldReturnForbidden(
        UserRole actorRole,
        UserRole targetCurrentRole,
        UserRole targetNewRole)
    {
        // Arrange
        var actor = await CreateUserAsync(role: actorRole);
        var target = await CreateUserAsync(tenantId: actor.TenantId, role: targetCurrentRole);
        Authenticate(actor);

        var request = new Request(Role: targetNewRole);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(target.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task UpdateUser_WhenModifyingSelf_ShouldReturnForbidden(UserRole actorRole)
    {
        // Arrange
        var actor = await CreateUserAsync(role: actorRole);
        Authenticate(actor);

        var request = new Request(Role: UserRole.Student);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(actor.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Validation ---
    [Fact]
    public async Task UpdateUser_WhenRoleIsInvalidEnum_ShouldReturnValidationProblem()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        var target = await CreateUserAsync(tenantId: admin.TenantId, role: UserRole.Student);
        Authenticate(admin);

        var request = new Request(Role: (UserRole)99);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(target.Id), request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Role));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task UpdateUser_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        const int nonExistentUserId = 99999;
        var request = new Request(Role: UserRole.Inspector);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(nonExistentUserId), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(nonExistentUserId));
    }

    [Fact]
    public async Task UpdateUser_WhenUserBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        var foreignUser = await CreateUserAsync(tenantId: TestTenants.Foreign, role: UserRole.Student);
        Authenticate(admin);

        var request = new Request(Role: UserRole.Inspector);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(foreignUser.Id), request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(foreignUser.Id));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator, UserRole.Student, UserRole.Inspector)]
    [InlineData(UserRole.Coordinator, UserRole.Inspector, UserRole.Student)]
    [InlineData(UserRole.SystemAdmin, UserRole.Student, UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin, UserRole.Coordinator, UserRole.Inspector)]
    public async Task UpdateUser_WhenRoleChangeIsWithinHierarchy_ShouldPersistAndReturnUpdatedUser(
        UserRole actorRole,
        UserRole initialRole,
        UserRole newRole)
    {
        // Arrange
        var actor = await CreateUserAsync(role: actorRole);
        var target = await CreateUserAsync(tenantId: actor.TenantId, role: initialRole);
        Authenticate(actor);

        var request = new Request(Role: newRole);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.ById(target.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().Be(target.Id);
        payload.Email.Should().Be(target.Email);
        payload.Role.Should().Be(newRole.ToString());

        var persisted = await QueryDbAsync(context => context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == target.Id));

        persisted.Should().NotBeNull();
        persisted.Role.Should().Be(newRole);
    }
}
