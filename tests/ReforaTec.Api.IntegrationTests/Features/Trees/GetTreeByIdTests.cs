using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Features.Trees.GetTreeById;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Trees;

public class GetTreeByIdTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetTreeById_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var tree = await CreateTreeAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.ById(tree.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTreeById_WhenTreeBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var foreignTree = await CreateTreeAsync(tenantId: TestTenants.Foreign);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.ById(foreignTree.Id));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(foreignTree.Id));
    }

    // --- 2. Validation ---
    // Validation: N/A — route constraint :int handles noninteger paths

    // --- 3. Business Errors ---
    [Fact]
    public async Task GetTreeById_WhenTreeDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        const int nonExistentTreeId = 99999;

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.ById(nonExistentTreeId));

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.TreeNotFound(nonExistentTreeId));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task GetTreeById_WhenTreeExists_ShouldReturnOkWithTreeDetailsAndAssignedStudents()
    {
        // Arrange
        var student = await CreateUserAsync();
        Authenticate(student);

        const string folio = "0042";
        const string notes = "Frente a biblioteca";

        var campaign = await CreateCampaignAsync();
        var expectedSpecies = await QueryDbAsync(context => context.Species.FirstAsync());
        var expectedVirtue = await QueryDbAsync(context => context.Virtues.FirstAsync());

        var tree = await CreateTreeAsync(
            speciesId: expectedSpecies.Id,
            virtueId: expectedVirtue.Id,
            folio: folio);

        await AssignTreeToUserAsync(tree.Id, student.Id, campaign.Id);
        var measurement = await RecordMeasurementAsync(tree.Id, student.Id);
        var inspection = await RecordInspectionAsync(tree.Id, student.Id, notes: notes);

        var expectedStudent = new AssignedStudentDto(
            Id: student.Id,
            ControlNumber: student.ControlNumber,
            GivenName: student.GivenName,
            FamilyName: student.FamilyName,
            SecondFamilyName: student.SecondFamilyName,
            DisplayName: student.DisplayName);

        var expectedResponse = new Response(
            Id: tree.Id,
            Folio: folio,

            HealthState: inspection.HealthState.ToString(),
            PlantingDate: tree.PlantingDate,
            Observations: notes,

            Virtue: expectedVirtue.Name,
            Species: expectedSpecies.CommonName,
            SpeciesScientificName: expectedSpecies.ScientificName,
            SpeciesImageUrl: expectedSpecies.ImageUrl,

            CurrentHeightCentimeters: measurement.HeightCentimeters,
            LatestPhotoUrl: measurement.PhotoUrl,

            Location: null,
            CurrentAssignedStudents: [expectedStudent]);

        // Act
        var response = await Client.GetAsync(ApiRoutes.Trees.ById(tree.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<Response>();
        result.Should().BeEquivalentTo(expectedResponse);
    }
}
