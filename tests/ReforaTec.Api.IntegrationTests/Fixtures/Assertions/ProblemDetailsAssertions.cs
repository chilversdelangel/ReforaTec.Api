using System.Net;
using System.Net.Http.Json;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Infrastructure.Mapping;

namespace ReforaTec.Api.IntegrationTests.Fixtures.Assertions;

public static class ProblemDetailsAssertions
{
    public static async Task ShouldBeProblemAsync(
        this HttpResponseMessage response,
        HttpStatusCode expectedStatusCode,
        Error expectedError)
    {
        response.StatusCode.Should().Be(expectedStatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        problem.Should().NotBeNull();
        problem.Type.Should().Be($"{ErrorTypes.Prefix}{expectedError.Code}");
        problem.Detail.Should().Be(expectedError.Description);
    }

    public static async Task ShouldBeValidationProblemAsync(
        this HttpResponseMessage response,
        params string[] expectedErrorKeys)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        
        problem.Should().NotBeNull();
        foreach (var key in expectedErrorKeys)
        {
            problem.Errors.Should().ContainKey(key);
        }
    }
}
