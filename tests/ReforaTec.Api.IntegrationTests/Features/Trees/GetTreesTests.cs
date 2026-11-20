using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Trees.GetTrees;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public class GetTreesTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetTrees_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTrees_WhenAuthenticatedAsStudent_ShouldReturnForbidden()
    {
        // Arrange
        var student = await CreateUserAsync();
        Authenticate(student);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.BaseRoute);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, Errors.Forbidden);
    }

    [Fact]
    public async Task GetTrees_ShouldExcludeTreesFromForeignTenants()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);

        var ownTree = await CreateTreeAsync();
        await CreateTreeAsync(tenantId: TestTenants.Foreign);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Trees.Should().ContainSingle(tree => tree.Id == ownTree.Id);
    }

    // --- 2. Happy Path ---
    [Fact]
    public async Task GetTrees_WhenAuthenticatedAsInspector_ShouldReturnTreesWithLocationAndActiveAssignedStudents()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);

        var student1 = await CreateUserAsync();
        var student2 = await CreateUserAsync();
        var inactiveStudent = await CreateUserAsync();

        var expectedLocation = new Location { Latitude = 22.253612, Longitude = -97.854291 };
        var tree = await CreateTreeAsync(location: expectedLocation);
        var campaign = await CreateCampaignAsync();
        await CreateCareAssignmentAsync(tree.Id, student1.Id, campaign.Id);
        await CreateCareAssignmentAsync(tree.Id, student2.Id, campaign.Id);
        await CreateCareAssignmentAsync(
            tree.Id,
            inactiveStudent.Id,
            campaign.Id,
            startDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            endDate: DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Trees.Should().ContainSingle();

        var item = payload.Trees[0];
        item.Id.Should().Be(tree.Id);
        item.Folio.Should().Be(tree.Folio);
        item.HealthState.Should().Be(nameof(TreeHealthState.Healthy));
        item.Location.Should().NotBeNull();
        item.Location.Latitude.Should().Be(expectedLocation.Latitude);
        item.Location.Longitude.Should().Be(expectedLocation.Longitude);
        item.AssignedStudents.Should().HaveCount(2);
        item.AssignedStudents.Select(student => student.Id).Should().BeEquivalentTo([student1.Id, student2.Id]);
    }

    [Fact]
    public async Task GetTrees_ShouldReturnTreesOrderedByFolioByDefault()
    {
        // Arrange
        var coordinator = await CreateUserAsync(role: UserRole.Coordinator);
        Authenticate(coordinator);

        var tree10 = await CreateTreeAsync(folio: "10");
        var tree01 = await CreateTreeAsync(folio: "01");
        var tree02 = await CreateTreeAsync(folio: "02");

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Trees.Select(tree => tree.Id).Should().ContainInOrder(
            tree01.Id,
            tree02.Id,
            tree10.Id);
    }
}
