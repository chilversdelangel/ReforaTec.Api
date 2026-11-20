using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.CareTypes.GetCareTypes;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Features.CareTypes;

public class GetCareTypesTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task GetCareTypes_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync(ApiRoutes.CareTypes.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // --- 2. Happy Path ---
    [Fact]
    public async Task GetCareTypes_WhenAuthenticatedAsStudent_ShouldReturnActiveCareTypesOrderedById()
    {
        // Arrange
        var student = await CreateUserAsync(role: UserRole.Student);
        Authenticate(student);

        var firstCareType = await CreateCareTypeAsync();
        var secondCareType = await CreateCareTypeAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.CareTypes.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.CareTypes.Select(careType => careType.Id).Should().BeInAscendingOrder();

        payload.CareTypes.Should().Contain(careType =>
            careType.Id == firstCareType.Id &&
            careType.Name == firstCareType.Name &&
            careType.IconUrl == firstCareType.IconUrl);

        payload.CareTypes.Should().Contain(careType =>
            careType.Id == secondCareType.Id &&
            careType.Name == secondCareType.Name &&
            careType.IconUrl == secondCareType.IconUrl);
    }

    [Fact]
    public async Task GetCareTypes_WhenAuthenticatedAsInspector_ShouldReturnSuccess()
    {
        // Arrange
        var inspector = await CreateUserAsync(role: UserRole.Inspector);
        Authenticate(inspector);

        var careType = await CreateCareTypeAsync();

        // Act
        var response = await Client.GetAsync(ApiRoutes.CareTypes.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.CareTypes.Should().Contain(item => item.Id == careType.Id);
    }

    // --- 3. Filtering Inactive Care Types ---
    [Fact]
    public async Task GetCareTypes_WhenCareTypeIsInactive_ShouldExcludeInactiveCareTypes()
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var inactiveCareType = await CreateCareTypeAsync(isActive: false);
        var activeCareType = await CreateCareTypeAsync(isActive: true);

        // Act
        var response = await Client.GetAsync(ApiRoutes.CareTypes.BaseRoute);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.CareTypes.Should().NotContain(careType => careType.Id == inactiveCareType.Id);
        payload.CareTypes.Should().Contain(careType => careType.Id == activeCareType.Id);
    }
}
