using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Users.GetUserProfile;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class GetUserProfileTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetUserProfile_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserProfile_WhenUserBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange: User is persisted in database under a foreign tenant
        var foreignUser = await CreateUserAsync(tenantId: TestTenants.Foreign);

        // Authenticate simulating an identity token bound to Primary tenant
        var tokenUser = new User
        {
            Id = foreignUser.Id,
            TenantId = TestTenants.Primary,
            CurrentRole = foreignUser.CurrentRole
        };
        Authenticate(tokenUser);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(foreignUser.Id));
    }

    // --- 2. Validation ---
    // Validation: N/A — no request body or query parameters

    // --- 3. Business Errors ---
    [Fact]
    public async Task GetUserProfile_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentUser = new User
        {
            Id = 99999,
            TenantId = TestTenants.Primary,
            CurrentRole = UserRole.Student,
            Email = "ghost@cdmadero.tecnm.mx"
        };
        Authenticate(nonExistentUser);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(nonExistentUser.Id));
    }

    [Fact]
    public async Task GetUserProfile_WhenUserIsSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var deletedUser = await CreateUserAsync(isDeleted: true);
        Authenticate(deletedUser);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(deletedUser.Id));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task GetUserProfile_WhenAuthenticated_ShouldReturnOkWithProfile()
    {
        // Arrange
        const string givenName = "Carlos";
        const string familyName = "López";
        const string controlNumber = "20010999";

        var user = await CreateUserAsync(
            givenName: givenName,
            familyName: familyName,
            controlNumber: controlNumber);

        Authenticate(user);

        var expectedProfile = new Response(
            GivenName: givenName,
            FamilyName: familyName,
            SecondFamilyName: null,
            DisplayName: null,
            ControlNumber: controlNumber,
            PhoneNumber: null,
            Gender: Gender.Male,
            Email: user.Email,
            CurrentCampaign: null,
            TotalTrees: 0,
            TotalServices: 0);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<Response>();
        profile.Should().BeEquivalentTo(expectedProfile);
    }

    [Fact]
    public async Task GetUserProfile_WhenUserHasActivity_ShouldReturnCalculatedStats()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var campaign = await CreateCampaignAsync();
        var tree = await CreateTreeAsync();

        await AddTreeToCampaignAsync(tree.Id, campaign.Id);
        await AssignTreeToUserAsync(tree.Id, user.Id, campaign.Id);
        await CreateTreeServiceAsync(tree.Id, user.Id, campaign.Id);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<Response>();
        profile.Should().NotBeNull();
        profile.TotalTrees.Should().Be(1);
        profile.TotalServices.Should().Be(1);
        profile.CurrentCampaign.Should().Be(campaign.CampaignName);
    }
}