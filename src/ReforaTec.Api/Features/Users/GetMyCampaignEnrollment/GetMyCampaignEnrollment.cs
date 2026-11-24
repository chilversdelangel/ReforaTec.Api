using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.GetMyCampaignEnrollment;

internal sealed class GetMyCampaignEnrollment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/me/campaign-enrollment", HandleRequest)
            .WithName("GetMyCampaignEnrollment")
            .WithTags(OpenApiTags.Users)
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Check active campaign enrollment")
            .WithDescription("Returns whether the authenticated user is currently enrolled in an active campaign.")
            .RequireAuthorization();
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

        var response = await Handler.Handle(identityResult.Value, context, cancellationToken);
        return Results.Ok(response);
    }
}
