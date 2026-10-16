using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.GetUserProfile;

internal sealed class GetUserProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/profile", HandleRequest)
            .WithName("GetUserProfile")
            .WithTags(OpenApiTags.Users)
            .RequireAuthorization()
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get authenticated user profile and stats")
            .WithDescription("Retrieves personal details and activity statistics (assigned trees count, completed services count, and active campaign name) for the currently authenticated user.");
    }

    private static async Task<IResult> HandleRequest(
        ICurrentUserService currentUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var identityResult = currentUser.GetIdentity();
        if (identityResult.IsError)
        {
            return identityResult.Errors.ToProblem();
        }

        var result = await Handler.Handle(
            identityResult.Value,
            context,
            cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}
