using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.CareAssignments.CloseCareAssignment;

internal sealed class CloseCareAssignment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/care-assignments/{assignmentId:int}", HandleRequest)
            .WithName("CloseCareAssignment")
            .WithTags(OpenApiTags.Campaigns)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Close a care assignment")
            .WithDescription("Terminates an active tree care assignment by setting its end date.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
        int assignmentId,
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

        var result = await Handler.Handle(identityResult.Value, assignmentId, request, context, cancellationToken);

        return result.Match(
            response => Results.Ok(response),
            errors => errors.ToProblem());
    }
}
