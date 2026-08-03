using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RevokeSession;

public static class RevokeSession
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/sessions/revoke", HandleRequest)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleRequest(
        Request request,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, jwtTokenService, cancellationToken);

        return result.Match(
            _ => Results.NoContent(),
            errors => errors.ToProblem());
    }
}
