using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Mapping;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

public static class GetCampaignById
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/campaign/{id:int}", HandleRequest)
            .WithName("GetCampaignById")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
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