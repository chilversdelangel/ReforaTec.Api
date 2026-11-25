using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.CloseCampaign;

internal sealed class CloseCampaign : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/campaigns/{campaignId:int}/close", HandleRequest)
            .WithName("CloseCampaign")
            .WithTags(OpenApiTags.Campaigns)
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Close a campaign")
            .WithDescription("Closes a campaign and terminates all remaining active tree care assignments.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        int campaignId,
        ICurrentUserService currentUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var identityResult = currentUser.GetIdentity();
        if (identityResult.IsError)
        {
            return identityResult.Errors.ToProblem();
        }

        var result = await Handler.Handle(identityResult.Value, campaignId, context, cancellationToken);

        return result.Match(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }
}
