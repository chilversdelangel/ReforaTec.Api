using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Users.GetUsers;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class GetUsersTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetUsers_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    public async Task GetUsers_WhenAuthenticatedAsUnauthorizedRole_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.BaseRoute);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    // --- 2. Happy Path ---
    [Theory]
    [InlineData(UserRole.Coordinator)]
    [InlineData(UserRole.SystemAdmin)]
    public async Task GetUsers_WhenAuthorized_ShouldReturnSubordinateTenantUsersOrderedByIdIncludingSoftDeleted(UserRole role)
    {
        // Arrange
        var actor = await CreateUserAsync(role: role);

        var activeUser = await CreateUserAsync();
        var deletedUser = await CreateUserAsync(isDeleted: true);
        var peerUser = await CreateUserAsync(role: role);
        var systemAdmin = await CreateUserAsync(role: UserRole.SystemAdmin);
        var foreignUser = await CreateUserAsync(tenantId: TestTenants.Foreign);

        Authenticate(actor);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Users.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<List<Response>>();

        payload.Should().NotBeNull();
        payload.Should().NotContain(item => item.Id == actor.Id);
        payload.Should().NotContain(item => item.Id == peerUser.Id);
        payload.Should().NotContain(item => item.Id == systemAdmin.Id);
        payload.Should().NotContain(item => item.Id == foreignUser.Id);
        payload.Should().OnlyContain(item => item.Role < role);
        payload.Select(item => item.Id).Should().BeInAscendingOrder();
        payload.Select(item => item.Id).Should().ContainInOrder(activeUser.Id, deletedUser.Id);

        var activeUserDto = payload.Single(item => item.Id == activeUser.Id);
        activeUserDto.Should().BeEquivalentTo(new
        {
            activeUser.Id,
            activeUser.GivenName,
            activeUser.FamilyName,
            activeUser.SecondFamilyName,
            activeUser.DisplayName,
            activeUser.ControlNumber,
            activeUser.PhoneNumber,
            activeUser.Gender,
            activeUser.Email,
            activeUser.IsEmailVerified,
            activeUser.Role,
            activeUser.IsDeleted,
            activeUser.DeletedAt
        });
        activeUserDto.CreatedAt.Should().BeCloseTo(activeUser.CreatedAt, TimeSpan.FromSeconds(1));
        activeUserDto.ModifiedAt.Should().BeCloseTo(activeUser.ModifiedAt, TimeSpan.FromSeconds(1));

        var deletedDto = payload.Single(item => item.Id == deletedUser.Id);
        deletedDto.IsDeleted.Should().BeTrue();
        deletedDto.DeletedAt.Should().NotBeNull();
    }
}
