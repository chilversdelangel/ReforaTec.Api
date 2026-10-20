using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Users.GetUserProfile;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class GetUserProfileTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetProfile_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentUser = new User
        {
            Id = 99999,
            TenantId = 1,
            CurrentRole = UserRole.Student,
            Email = "ghost@cdmadero.tecnm.mx"
        };
        Authenticate(nonExistentUser);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Type.Should().Be("https://api.reforatec.com/errors/user/not-found");
    }

    [Fact]
    public async Task GetProfile_WhenUserIsSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var deletedUser = await CreateUserAsync(isDeleted: true);
        Authenticate(deletedUser);

        // Act
        var response = await Client.GetAsync("/api/v1/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Type.Should().Be("https://api.reforatec.com/errors/user/not-found");
    }

    [Fact]
    public async Task GetProfile_WhenAuthenticated_ShouldReturnOkWithProfile()
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
    public async Task GetProfile_WhenUserHasActivity_ShouldReturnCalculatedStats()
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