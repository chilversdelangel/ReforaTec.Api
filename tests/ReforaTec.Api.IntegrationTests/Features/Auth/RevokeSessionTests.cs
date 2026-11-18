using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReforaTec.Api.Database;
using ReforaTec.Api.Features.Auth.RevokeSession;
using ReforaTec.Api.Infrastructure.Security.Jwt;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Auth;

public sealed class RevokeSessionTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    // Security: N/A — anonymous endpoint (AllowAnonymous)

    // --- 2. Validation ---
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task RevokeSession_WithInvalidRefreshToken_ShouldReturnBadRequest(string? refreshToken)
    {
        // Arrange
        var request = new Request(refreshToken!);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RevokeSession, request);

        // Assert
        await response.ShouldBeValidationProblemAsync("RefreshToken");
    }

    // --- 3. Business Errors ---
    // Business Errors: N/A — idempotent revocation (always 204 NoContent even if token does not exist)

    // --- 4. Happy Path ---
    [Fact]
    public async Task RevokeSession_WhenTokenDoesNotExist_ShouldReturnNoContent()
    {
        // Arrange
        var request = new Request("non-existent-refresh-token");

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RevokeSession, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RevokeSession_WithExistingToken_ShouldDeleteTokenAndReturnNoContent()
    {
        // Arrange
        var user = await CreateUserAsync(email: "revoke@campus.edu");
        var rawToken = "raw-refresh-token-to-revoke";
        var hashedToken = HashRefreshToken(rawToken);
        await CreateRefreshTokenAsync(user.Id, hashedToken);

        var request = new Request(rawToken);

        // Act
        var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.RevokeSession, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = Factory.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenExists = await appDbContext.RefreshTokens.AnyAsync(token => token.HashedToken == hashedToken);
        tokenExists.Should().BeFalse();
    }

    private string HashRefreshToken(string rawToken)
    {
        using var scope = Factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        return jwtService.HashRefreshToken(rawToken);
    }
}
