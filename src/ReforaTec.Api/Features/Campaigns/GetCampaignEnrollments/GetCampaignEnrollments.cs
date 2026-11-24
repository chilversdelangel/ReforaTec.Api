using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignEnrollments;

internal sealed class GetCampaignEnrollments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/campaigns/{campaignId:int}/enrollments", HandleRequest)
            .WithName("GetCampaignEnrollments")
            .WithTags(OpenApiTags.Campaigns)
            .Produces<IReadOnlyList<Response>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("List enrollments for a campaign")
            .WithDescription("Returns all student enrollments for the specified campaign within the authenticated user's tenant.")
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
