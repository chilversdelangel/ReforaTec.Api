using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.CareTypes.GetCareTypes;

internal sealed class GetCareTypes : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/care-types", HandleRequest)
            .WithName("GetCareTypes")
            .WithTags(OpenApiTags.Catalogs)
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("List active care types")
            .WithDescription("Retrieves the catalog of active tree care types and icons for mobile care logging.")
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

        var result = await Handler.Handle(context, cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}
