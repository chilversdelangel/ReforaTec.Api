using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Features.Auth.RefreshSession;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Auth;

public sealed class RefreshSessionTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    // Security: N/A — anonymous endpoint (AllowAnonymous)

    // --- 2. Validation ---
    [Theory]
    [InlineData("", Audience.StudentMobileApp, "RefreshToken")]
    [InlineData("   ", Audience.StudentMobileApp, "RefreshToken")]
    [InlineData("valid-raw-token", "unknown-audience", "Audience")]
    public async Task RefreshSession_WithInvalidInput_ShouldReturnBadRequest(
        string refreshToken,
        string audience,
        string expectedField)
    {
        // Arrange
        var request = new Request(refreshToken, audience);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RefreshSession, request);

        // Assert
        await response.ShouldBeValidationProblemAsync(expectedField);
    }

    // --- 3. Business Errors ---
    [Fact]
    public async Task RefreshSession_WhenTokenDoesNotExist_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new Request("non-existent-refresh-token", Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RefreshSession, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, Errors.InvalidToken);
    }

    [Fact]
    public async Task RefreshSession_WhenAudienceMismatches_ShouldReturnUnauthorized()
    {
        // Arrange
        var user = await CreateUserAsync(email: "mismatch@campus.edu");
        var rawToken = "raw-refresh-token-mismatch";
        var hashedToken = HashRefreshToken(rawToken);
        await CreateRefreshTokenAsync(user.Id, hashedToken);

        var request = new Request(rawToken, Audience.WebDashboard);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RefreshSession, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, Errors.InvalidToken);
    }

    [Fact]
    public async Task RefreshSession_WhenTokenExpired_ShouldReturnUnauthorizedAndDeleteToken()
    {
        // Arrange
        var user = await CreateUserAsync(email: "expired-refresh@campus.edu");
        var rawToken = "raw-refresh-token-expired";
        var hashedToken = HashRefreshToken(rawToken);
        await CreateRefreshTokenAsync(user.Id, hashedToken, Audience.StudentMobileApp, DateTime.UtcNow.AddMinutes(-5));

        var request = new Request(rawToken, Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RefreshSession, request);

        // Assert
        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, Errors.TokenExpired);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenExists = await appDbContext.RefreshTokens.AnyAsync(token => token.HashedToken == hashedToken);
        tokenExists.Should().BeFalse();
    }

    // --- 4. Happy Path ---
    [Fact]
    public async Task RefreshSession_WithValidToken_ShouldRotateRefreshTokenAndReturnNewTokens()
    {
        // Arrange
        var user = await CreateUserAsync(email: "valid-refresh@campus.edu");
        var oldRawToken = "raw-refresh-token-valid";
        var oldHashedToken = HashRefreshToken(oldRawToken);
        await CreateRefreshTokenAsync(user.Id, oldHashedToken);

        var request = new Request(oldRawToken, Audience.StudentMobileApp);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RefreshSession, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<Response>();
        payload.Should().NotBeNull();
        payload.AccessToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBeNullOrWhiteSpace();
        payload.RefreshToken.Should().NotBe(oldRawToken);

        var newRawToken = payload.RefreshToken;
        var newHashedToken = HashRefreshToken(newRawToken);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var oldTokenExists = await appDbContext.RefreshTokens.AnyAsync(token => token.HashedToken == oldHashedToken);
        var newTokenExists = await appDbContext.RefreshTokens.AnyAsync(token => token.HashedToken == newHashedToken);

        oldTokenExists.Should().BeFalse();
        newTokenExists.Should().BeTrue();
    }

    private string HashRefreshToken(string rawToken)
    {
        using var scope = Factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        return jwtService.HashRefreshToken(rawToken);
    }
}
