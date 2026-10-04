using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RevokeSession;

internal sealed class RevokeSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/sessions/revoke", HandleRequest)
            .WithTags(OpenApiTags.Auth)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status204NoContent)
            .WithSummary("Revoke user session")
            .WithDescription("Revokes an active refresh token, ending the associated device session.");
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