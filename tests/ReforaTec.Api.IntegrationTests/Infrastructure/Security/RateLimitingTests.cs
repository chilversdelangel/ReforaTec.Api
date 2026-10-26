using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Features.Auth.RequestOtp;
using ReforaTec.Api.IntegrationTests.Fixtures;

namespace ReforaTec.Api.IntegrationTests.Infrastructure.Security;

public class RateLimitingTests(ReforaTecWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    // --- 1. Security & Rate Limiting ---
    [Fact]
    public async Task RequestOtp_WhenExceedingEmailLimit_ShouldReturnTooManyRequests()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.1");
        const string email = "student.target@campus.edu";
        var request = new Request(email);

        // Act & Assert: First 3 consecutive requests should succeed
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var successfulResponse = await Client.PostAsJsonAsync(ApiRoutes.Auth.OtpCodes, request);
            successfulResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Act: 4th request with the same email exceeds the email rate limit (3 req/5 min)
        var rateLimitedResponse = await Client.PostAsJsonAsync(ApiRoutes.Auth.OtpCodes, request);

        // Assert: Returns 429 Too Many Requests with Retry-After header and ProblemDetails
        rateLimitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rateLimitedResponse.Headers.RetryAfter.Should().NotBeNull();
        rateLimitedResponse.Headers.RetryAfter!.Delta.Should().BeGreaterThan(TimeSpan.Zero);

        var problemDetails = await rateLimitedResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be((int)HttpStatusCode.TooManyRequests);
        problemDetails.Title.Should().Be("Too Many Requests");
    }

    [Fact]
    public async Task RequestOtp_WhenDifferentEmailsUnderSameIp_ShouldAllowUpToIpLimit()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.2");
        var distinctEmails = new[]
        {
            "user1@campus.edu",
            "user2@campus.edu",
            "user3@campus.edu",
            "user4@campus.edu",
            "user5@campus.edu"
        };

        // Act & Assert: Each distinct email is permitted and not blocked by previous requests
        foreach (var email in distinctEmails)
        {
            var request = new Request(email);
            var response = await Client.PostAsJsonAsync(ApiRoutes.Auth.OtpCodes, request);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }

    [Fact]
    public async Task RequestOtp_WhenExceedingIpLimit_ShouldReturnTooManyRequests()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.3");

        // Act & Assert: First 30 requests with unique emails should succeed under the IP limit
        for (var attempt = 1; attempt <= 30; attempt++)
        {
            var request = new Request($"user{attempt}@campus.edu");
            var successfulResponse = await Client.PostAsJsonAsync(ApiRoutes.Auth.OtpCodes, request);

            successfulResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Act: 31st request from the same client IP exceeds the IP limit (30 req/min)
        var exceedingRequest = new Request("user31@campus.edu");
        var rateLimitedResponse = await Client.PostAsJsonAsync(ApiRoutes.Auth.OtpCodes, exceedingRequest);

        // Assert: Returns 429 Too Many Requests with Retry-After header and ProblemDetails
        rateLimitedResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        rateLimitedResponse.Headers.RetryAfter.Should().NotBeNull();
        rateLimitedResponse.Headers.RetryAfter!.Delta.Should().BeGreaterThan(TimeSpan.Zero);

        var problemDetails = await rateLimitedResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be((int)HttpStatusCode.TooManyRequests);
        problemDetails.Title.Should().Be("Too Many Requests");
    }

    // --- 2. Validation ---
    // Validation: Covered in Features/Auth/RequestOtpTests.cs

    // --- 3. Business Errors ---
    // Business Errors: Covered in Features/Auth/RequestOtpTests.cs

    // --- 4. Happy Path ---
    // Happy Path: Covered in Features/Auth/RequestOtpTests.cs
}
