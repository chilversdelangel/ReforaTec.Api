using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RotateToken;

public static class RotateToken
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/tokens", HandleRequest)
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
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, jwtTokenService, cancellationToken);

        return result.Match(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }
}
