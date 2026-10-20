using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ReforaTec.Api.Features.Auth.RequestOtp;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Features.Auth;

public class RequestOtpTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem.Errors.Should().ContainKey("Email");
    }

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