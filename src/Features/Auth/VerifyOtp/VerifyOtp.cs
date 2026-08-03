using Microsoft.Extensions.Options;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public static class VerifyOtp
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/sessions", HandleRequest)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .ProducesValidationProblem()
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
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
