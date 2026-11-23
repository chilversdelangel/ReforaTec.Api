using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.CampaignEnrollments.EnrollInCampaign;

internal sealed class EnrollInCampaign : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/campaign-enrollments", HandleRequest)
            .WithName("EnrollInCampaign")
            .WithTags(OpenApiTags.Campaigns)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Enroll in a campaign")
            .WithDescription("Enrolls the authenticated student into an active campaign using its enrollment code.")
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleRequest(
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

        var result = await Handler.Handle(identityResult.Value, request, context, cancellationToken);

        return result.Match(
            response => Results.Created($"/v1/campaign-enrollments/{response.Id}", response),
            errors => errors.ToProblem());
    }
}
