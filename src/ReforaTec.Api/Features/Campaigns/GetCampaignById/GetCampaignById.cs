using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

internal sealed class GetCampaignById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/campaigns/{id:int}", HandleRequest)
            .WithTags(OpenApiTags.Campaigns)
            .WithName("GetCampaignById")
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get campaign by ID")
            .WithDescription("Retrieves the details of an environmental campaign by its unique identifier.");
    }

    private static async Task<IResult> HandleRequest(int id, AppDbContext context)
    {
        var result = await Handler.Handle(id, context);

        return result.Match(
            value => Results.Ok(value),
            errors => errors.ToProblem()
        );
    }
}