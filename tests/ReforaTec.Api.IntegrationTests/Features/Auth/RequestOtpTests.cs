using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReforaTec.Api.Features.Auth.RequestOtp;
using ReforaTec.Api.IntegrationTests.Fixtures;
using ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

namespace ReforaTec.Api.IntegrationTests.Features.Auth;

public class RequestOtpTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security ---
    // Security: N/A — anonymous endpoint (AllowAnonymous)

    // --- 2. Validation ---
    [Theory]
    [InlineData("invalid-email")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task RequestOtp_WithInvalidEmail_ShouldReturnBadRequest(string? email)
    {
        // Arrange
        var request = new Request(email!);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/otp-codes", request);

        // Assert
        await response.ShouldBeValidationProblemAsync("Email");
    }

    // --- 3. Business Errors ---
    // Business Errors: N/A — OWASP anti-enumeration (always 204 NoContent)

    // --- 4. Happy Path ---
    [Fact]
    public async Task RequestOtp_WithValidEmail_ShouldReturnNoContent()
    {
        // Arrange
        var request = new Request("student@campus.edu");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/otp-codes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}