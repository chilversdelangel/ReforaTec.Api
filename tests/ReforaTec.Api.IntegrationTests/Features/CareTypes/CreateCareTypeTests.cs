using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.CareTypes.CreateCareType;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.CareTypes;

public class CreateCareTypeTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private const string ValidIconUrl = "https://cdn.reforatec.org/care-types/water.png";

    // --- 1. Security ---
    [Fact]
    public async Task CreateCareType_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(Name: "Riego Profundo", IconUrl: ValidIconUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CareTypes.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(UserRole.Student)]
    [InlineData(UserRole.Inspector)]
    [InlineData(UserRole.Coordinator)]
    public async Task CreateCareType_WhenAuthenticatedAsNonAdmin_ShouldReturnForbidden(UserRole role)
    {
        // Arrange
        var user = await CreateUserAsync(role: role);
        Authenticate(user);

        var request = new Request(Name: "Riego Profundo", IconUrl: ValidIconUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CareTypes.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("NombreDeTipoDeCuidadoDemasiadoLargoQueSuperaCincuentaCaracteres")]
    public async Task CreateCareType_WhenNameIsInvalid_ShouldReturnValidationProblem(string invalidName)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var request = new Request(Name: invalidName, IconUrl: ValidIconUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CareTypes.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateCareType_WhenIconUrlIsInvalid_ShouldReturnValidationProblem(string invalidIconUrl)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var request = new Request(Name: "Riego Profundo", IconUrl: invalidIconUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CareTypes.BaseRoute, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.IconUrl));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task CreateCareType_WhenNameAlreadyExists_ShouldReturnConflict()
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        var careTypeName = $"Cuidado-{Guid.NewGuid().ToString("N")[..6]}";
        await CreateCareTypeAsync(name: careTypeName);
        Authenticate(admin);

        var upperName = careTypeName.ToUpperInvariant();
        var request = new Request(Name: $"  {upperName}  ", IconUrl: ValidIconUrl);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CareTypes.BaseRoute, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.DuplicateName(upperName));
    }

    // --- 4. Happy Path ---
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCareType_WhenValid_ShouldPersistAndReturnCreated(bool isActive)
    {
        // Arrange
        var admin = await CreateUserAsync(role: UserRole.SystemAdmin);
        Authenticate(admin);

        var careTypeName = $"Deshierbe-{Guid.NewGuid().ToString("N")[..6]}";
        var request = new Request(Name: $"  {careTypeName}  ", IconUrl: ValidIconUrl, IsActive: isActive);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.CareTypes.BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.Id.Should().BeGreaterThan(0);
        payload.Name.Should().Be(careTypeName.ToSanitized());
        payload.IconUrl.Should().Be(ValidIconUrl);
        payload.IsActive.Should().Be(isActive);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().EndWith($"{ApiRoutes.CareTypes.BaseRoute}/{payload.Id}");

        var persisted = await QueryDbAsync(context => context.CareTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(careType => careType.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.Name.Should().Be(careTypeName);
        persisted.NormalizedName.Should().Be(careTypeName.ToNormalized());
        persisted.IconUrl.Should().Be(ValidIconUrl);
        persisted.IsActive.Should().Be(isActive);
    }
}
