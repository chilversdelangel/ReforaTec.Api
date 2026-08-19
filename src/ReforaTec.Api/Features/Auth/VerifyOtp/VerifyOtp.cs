using Microsoft.Extensions.Options;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

internal sealed class VerifyOtp : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/sessions", HandleRequest)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Create session / Verify OTP")
            .WithDescription("Verifies an OTP verification code and generates a JWT access token and refresh token.");
    }

    private static async Task<IResult> HandleRequest(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        IOptionsSnapshot<JwtOptions> jwtOptions,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, jwtTokenService, jwtOptions, cancellationToken);

        return result.Match(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }
}