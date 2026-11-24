using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Authorization;

namespace ReforaTec.Api.Features.CareTypes.CreateCareType;

internal sealed class CreateCareType : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/care-types", HandleRequest)
            .WithName("CreateCareType")
            .WithTags(OpenApiTags.Catalogs)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Create a care type")
            .WithDescription("Creates a new tree care activity type in the global catalog.")
            .RequireAuthorization(Policy.CanManageCatalogs);
    }

    private static async Task<IResult> HandleRequest(
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, cancellationToken);

        return result.Match(
            response => Results.Created($"/v1/care-types/{response.Id}", response),
            errors => errors.ToProblem());
    }
}
