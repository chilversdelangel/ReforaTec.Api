using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Users.UpdateUserProfile;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Users;

public class UpdateUserProfileTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    [Fact]
    public async Task UpdateUserProfile_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request(GivenName: "Updated");

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenUserBelongsToForeignTenant_ShouldReturnNotFound()
    {
        // Arrange
        // Multi-tenant regression: Verifies database query strictly filters by TenantId and not solely by UserId
        var user = await CreateUserAsync();
        AuthenticateWithClaims(user.Id, tenantId: TestTenants.Foreign);

        var request = new Request(GivenName: "HackedName");

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(user.Id));
    }

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateUserProfile_WhenGivenNameIsEmpty_ShouldReturnBadRequest(string invalidGivenName)
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var request = new Request(GivenName: invalidGivenName);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.GivenName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateUserProfile_WhenFamilyNameIsEmpty_ShouldReturnBadRequest(string invalidFamilyName)
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var request = new Request(FamilyName: invalidFamilyName);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.FamilyName));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("not-a-number")]
    [InlineData("+123456")]
    [InlineData("+1234567890123456")]
    public async Task UpdateUserProfile_WhenPhoneNumberIsInvalid_ShouldReturnBadRequest(string invalidPhone)
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var request = new Request(PhoneNumber: invalidPhone);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.PhoneNumber));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(99)]
    public async Task UpdateUserProfile_WhenGenderIsInvalid_ShouldReturnBadRequest(int invalidGender)
    {
        // Arrange
        var user = await CreateUserAsync();
        Authenticate(user);

        var request = new Request(Gender: (Gender)invalidGender);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Gender));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task UpdateUserProfile_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        const int nonExistentUserId = 99999;
        AuthenticateWithClaims(userId: nonExistentUserId);

        var request = new Request(GivenName: "GhostName");

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(nonExistentUserId));
    }

    [Fact]
    public async Task UpdateUserProfile_WhenUserIsSoftDeleted_ShouldReturnNotFound()
    {
        // Arrange
        var deletedUser = await CreateUserAsync(isDeleted: true);
        Authenticate(deletedUser);

        var request = new Request(GivenName: "DeletedName");

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(deletedUser.Id));
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task UpdateUserProfile_WhenNoFieldsProvided_ShouldReturnOkWithUnmodifiedProfile()
    {
        // Arrange
        var user = await CreateUserAsync();
        
        Authenticate(user);

        var request = new Request();

        var expectedResponse = new Response(
            Id: user.Id,
            GivenName: user.GivenName,
            FamilyName: user.FamilyName,
            SecondFamilyName: user.SecondFamilyName,
            DisplayName: user.DisplayName,
            ControlNumber: user.ControlNumber,
            PhoneNumber: user.PhoneNumber,
            Gender: user.Gender.ToString(),
            Email: user.Email,
            Role: user.Role.ToString());

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenValidDataProvided_ShouldReturnOkAndPersistChanges()
    {
        // Arrange
        const string phoneNumber = "8331234567";
        
        var user = await CreateUserAsync(
            givenName: "OriginalGiven",
            familyName: "OriginalFamily",
            gender: Gender.Male);
        Authenticate(user);
        
        var request = new Request(
            GivenName: "UpdatedGiven",
            FamilyName: "UpdatedFamily",
            SecondFamilyName: "UpdatedSecond",
            DisplayName: "CoolStudent",
            PhoneNumber: phoneNumber,
            Gender: Gender.Female);

        var expectedResponse = new Response(
            Id: user.Id,
            GivenName: request.GivenName!,
            FamilyName: request.FamilyName!,
            SecondFamilyName: request.SecondFamilyName,
            DisplayName: request.DisplayName,
            ControlNumber: user.ControlNumber,
            PhoneNumber: $"+52{phoneNumber}",
            Gender: request.Gender!.Value.ToString(),
            Email: user.Email,
            Role: user.Role.ToString());

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().BeEquivalentTo(expectedResponse);

        // Verify database persistence
        var persisted = await QueryDbAsync(context =>
            context.Users.AsNoTracking().FirstOrDefaultAsync(userEntity => userEntity.Id == user.Id));

        persisted.Should().NotBeNull();
        persisted.Should().BeEquivalentTo(expectedResponse, options => options
            .Excluding(userEntity => userEntity.Gender)
            .Excluding(userEntity => userEntity.Role));
        
        persisted.Gender.ToString().Should().Be(expectedResponse.Gender);
        persisted.Role.ToString().Should().Be(expectedResponse.Role);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenClearingOptionalFields_ShouldPersistNullValues()
    {
        // Arrange
        const string phoneNumber = "8331112233";

        var user = await CreateUserAsync(
            secondFamilyName: "PreviousSecond",
            displayName: "PreviousNick",
            phoneNumber: phoneNumber);
        Authenticate(user);

        var request = new Request(
            SecondFamilyName: string.Empty,
            DisplayName: string.Empty);

        // Act
        var response = await Client.PatchAsJsonAsync(ApiRoutes.Users.Me, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var persisted = await QueryDbAsync(context =>
            context.Users.AsNoTracking().FirstOrDefaultAsync(userEntity => userEntity.Id == user.Id));

        persisted.Should().NotBeNull();
        persisted.SecondFamilyName.Should().BeNull();
        persisted.DisplayName.Should().BeNull();
        persisted.PhoneNumber.Should().Be($"+52{phoneNumber}"); // Unmodified
    }
}
