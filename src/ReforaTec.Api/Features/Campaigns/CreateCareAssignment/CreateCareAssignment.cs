using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Campaigns.CreateCareAssignment;

internal sealed class CreateCareAssignment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/campaigns/{campaignId:int}/care-assignments", HandleRequest)
            .WithName("CreateCareAssignment")
            .WithTags(OpenApiTags.Campaigns)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Assign a tree to a student in a campaign")
            .WithDescription("Creates an active care assignment linking an enrolled student to a tree within an active campaign.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        int campaignId,
        Request request,
        ICurrentUserService currentUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var identityResult = currentUser.GetIdentity();
        if (identityResult.IsError)
        {
            return identityResult.Errors.ToProblem();
        }

        var result = await Handler.Handle(identityResult.Value, campaignId, request, context, cancellationToken);

        return result.Match(
            response => Results.Created($"/v1/care-assignments/{response.Id}", response),
            errors => errors.ToProblem());
    }
}
