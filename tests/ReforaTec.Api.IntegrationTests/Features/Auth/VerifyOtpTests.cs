using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Features.Auth.VerifyOtp;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Auth;

public sealed class VerifyOtpTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    // Security: N/A — anonymous endpoint (AllowAnonymous)

    // --- 2. Validation ---
    [Theory]
    [InlineData("invalid-email", "123456", Audience.StudentMobileApp, "Email")]
    [InlineData("student@campus.edu", "123", Audience.StudentMobileApp, "OtpCode")]
    [InlineData("student@campus.edu", "123456", "unknown-audience", "Audience")]
    public async Task VerifyOtp_WithInvalidInput_ShouldReturnBadRequest(
        string email,
        string otpCode,
        string audience,
        string expectedField)
    {
        // Arrange
        var request = new Request(email, otpCode, audience);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Sessions, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(expectedField);
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task VerifyOtp_WhenUserDoesNotExist_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request("nonexistent@campus.edu", "123456", Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Sessions, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, Errors.InvalidCredentials);
    }

    [Fact]
    public async Task VerifyOtp_WhenOtpExpired_ShouldReturnUnauthorizedAndDeleteOtp()
    {
        // Arrange
        var user = await CreateUserAsync(email: "expired@campus.edu");
        await CreateOneTimePasswordAsync(user.Id, code: "123456", expiresAt: DateTime.UtcNow.AddMinutes(-5));
        var request = new Request(user.Email, "123456", Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Sessions, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, Errors.OtpExpired);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var otpExists = await appDbContext.OneTimePasswords.AnyAsync(otp => otp.UserId == user.Id);
        otpExists.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyOtp_WhenMaxFailedAttemptsReached_ShouldReturnUnauthorizedAndDeleteOtp()
    {
        // Arrange
        var user = await CreateUserAsync(email: "locked@campus.edu");
        await CreateOneTimePasswordAsync(user.Id, code: "123456", failedAttempts: 2);
        var request = new Request(user.Email, "999999", Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Sessions, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, Errors.TooManyAttempts);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var otpExists = await appDbContext.OneTimePasswords.AnyAsync(otp => otp.UserId == user.Id);
        otpExists.Should().BeFalse();
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task VerifyOtp_WithValidCode_ShouldReturnTokensVerifyEmailAndDeleteOtp()
    {
        // Arrange
        var user = await CreateUserAsync(email: "valid@campus.edu", isEmailVerified: false);
        await CreateOneTimePasswordAsync(user.Id, code: "654321");
        var request = new Request(user.Email, "654321", Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Sessions, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.TokenType.Should().Be("Bearer");
        payload.ExpiresInSeconds.Should().BeGreaterThan(0);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var refreshedUser = await appDbContext.Users.FirstAsync(candidate => candidate.Id == user.Id);
        var otpExists = await appDbContext.OneTimePasswords.AnyAsync(otp => otp.UserId == user.Id);
        var sessionCount = await appDbContext.RefreshTokens.CountAsync(token => token.UserId == user.Id);

        refreshedUser.IsEmailVerified.Should().BeTrue();
        otpExists.Should().BeFalse();
        sessionCount.Should().Be(1);
    }

    [Fact]
    public async Task VerifyOtp_WhenMaxSessionsReached_ShouldEvictOldestSession()
    {
        // Arrange
        var user = await CreateUserAsync(email: "sessions@campus.edu");
        await CreateOneTimePasswordAsync(user.Id, code: "654321");
        await CreateRefreshTokenAsync(user.Id);
        await CreateRefreshTokenAsync(user.Id);

        var request = new Request(user.Email, "654321", Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.Sessions, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sessionCount = await appDbContext.RefreshTokens.CountAsync(token => token.UserId == user.Id);
        sessionCount.Should().Be(2);
    }
}
