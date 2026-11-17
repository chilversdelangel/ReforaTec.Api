using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Features.Auth.RegisterUser;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Auth;

public class RegisterUserTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    // Security: N/A — anonymous endpoint (AllowAnonymous)

    // --- 2. Validation ---
    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterUser_WithInvalidEmail_ShouldReturnBadRequest(string invalidEmail)
    {
        // Arrange
        var request = new Request(
            Email: invalidEmail,
            GivenName: "Juan",
            FamilyName: "Pérez",
            SecondFamilyName: null,
            ControlNumber: "20010999",
            PhoneNumber: null,
            Gender: Gender.Male);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterUser_WithMissingGivenName_ShouldReturnBadRequest(string invalidGivenName)
    {
        // Arrange
        var request = new Request(
            Email: "student@cdmadero.tecnm.mx",
            GivenName: invalidGivenName,
            FamilyName: "Pérez",
            SecondFamilyName: null,
            ControlNumber: "20010999",
            PhoneNumber: null,
            Gender: Gender.Male);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.GivenName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterUser_WithMissingFamilyName_ShouldReturnBadRequest(string invalidFamilyName)
    {
        // Arrange
        var request = new Request(
            Email: "student@cdmadero.tecnm.mx",
            GivenName: "Juan",
            FamilyName: invalidFamilyName,
            SecondFamilyName: null,
            ControlNumber: "20010999",
            PhoneNumber: null,
            Gender: Gender.Male);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.FamilyName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterUser_WithMissingControlNumber_ShouldReturnBadRequest(string invalidControlNumber)
    {
        // Arrange
        var request = new Request(
            Email: "student@cdmadero.tecnm.mx",
            GivenName: "Juan",
            FamilyName: "Pérez",
            SecondFamilyName: null,
            ControlNumber: invalidControlNumber,
            PhoneNumber: null,
            Gender: Gender.Male);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.ControlNumber));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("letters-not-allowed")]
    [InlineData("+123456")]
    [InlineData("+1234567890123456")]
    public async Task RegisterUser_WithInvalidPhoneNumber_ShouldReturnBadRequest(string invalidPhone)
    {
        // Arrange
        var request = new Request(
            Email: "student@cdmadero.tecnm.mx",
            GivenName: "Juan",
            FamilyName: "Pérez",
            SecondFamilyName: null,
            ControlNumber: "20010999",
            PhoneNumber: invalidPhone,
            Gender: Gender.Male);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.PhoneNumber));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(99)]
    public async Task RegisterUser_WithInvalidGender_ShouldReturnBadRequest(int invalidGender)
    {
        // Arrange
        var request = new Request(
            Email: "student@cdmadero.tecnm.mx",
            GivenName: "Juan",
            FamilyName: "Pérez",
            SecondFamilyName: null,
            ControlNumber: "20010999",
            PhoneNumber: null,
            Gender: (Gender)invalidGender);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(nameof(Request.Gender));
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task RegisterUser_WhenEmailAlreadyExistsAndVerified_ShouldReturnConflict()
    {
        // Arrange
        const string existingEmail = "existing.student@cdmadero.tecnm.mx";
        await CreateUserAsync(email: existingEmail, isEmailVerified: true);

        var request = new Request(
            Email: existingEmail,
            GivenName: "New",
            FamilyName: "Attempt",
            SecondFamilyName: null,
            ControlNumber: "21079999",
            PhoneNumber: null,
            Gender: Gender.Female);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.EmailAlreadyExists);
    }

    [Fact]
    public async Task RegisterUser_WhenControlNumberAlreadyExistsAndVerified_ShouldReturnConflict()
    {
        // Arrange
        const string existingControlNumber = "20010888";
        
        await CreateUserAsync(
            email: "verified.first@cdmadero.tecnm.mx",
            controlNumber: existingControlNumber,
            isEmailVerified: true);

        var request = new Request(
            Email: "different.student@cdmadero.tecnm.mx",
            GivenName: "New",
            FamilyName: "Attempt",
            SecondFamilyName: null,
            ControlNumber: existingControlNumber,
            PhoneNumber: null,
            Gender: Gender.Male);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, Errors.ControlNumberAlreadyExists);
    }

    [Fact]
    public async Task RegisterUser_WhenInstitutionalDomainNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = new Request(
            Email: "user@unknown-domain.com",
            GivenName: "External",
            FamilyName: "User",
            SecondFamilyName: null,
            ControlNumber: "99999999",
            PhoneNumber: null,
            Gender: Gender.Other);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.InstitutionalDomainNotFound);
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task RegisterUser_WithValidData_ShouldReturnCreatedAndPersistUserWithStudentRole()
    {
        // Arrange
        const string phoneNumber = "8339876543";
        
        var request = new Request(
            Email: "new.student@cdmadero.tecnm.mx",
            GivenName: "Carlos",
            FamilyName: "Mendoza",
            SecondFamilyName: "Garza",
            ControlNumber: "21070555",
            PhoneNumber: phoneNumber,
            Gender: Gender.Male);

        var expectedResponse = new Response(
            Id: 0,
            TenantId: TestTenants.Primary,
            Email: request.Email,
            GivenName: request.GivenName,
            FamilyName: request.FamilyName,
            SecondFamilyName: request.SecondFamilyName,
            DisplayName: null,
            ControlNumber: request.ControlNumber,
            PhoneNumber: $"+52{phoneNumber}",
            Gender: request.Gender,
            Role: UserRole.Student,
            CreatedAt: default);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Register, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = await response.Content.ReadFromJsonAsync<Response>();
        
        payload.Should().NotBeNull();
        payload.Id.Should().BeGreaterThan(0);
        payload.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        payload.Should().BeEquivalentTo(expectedResponse, options => options
            .Excluding(expected => expected.Id)
            .Excluding(expected => expected.CreatedAt));

        // Verify database persistence
        var persisted = await QueryDbAsync(context =>
            context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == payload.Id));

        persisted.Should().NotBeNull();
        persisted.Should().BeEquivalentTo(expectedResponse, options => options
            .Excluding(expected => expected.Id)
            .Excluding(expected => expected.CreatedAt));
        persisted.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        persisted.IsEmailVerified.Should().BeFalse();
    }
}
