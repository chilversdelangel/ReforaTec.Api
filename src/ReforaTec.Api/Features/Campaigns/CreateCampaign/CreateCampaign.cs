using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

internal sealed class CreateCampaign : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/campaigns", HandleRequest)
            .WithTags(OpenApiTags.Campaigns)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .ProducesValidationProblem()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Create campaign")
            .WithDescription("Creates a new institutional campaign with start/end dates and inscription code.");
    }

    private static async Task<IResult> HandleRequest(Request request, AppDbContext context)
    {
        var result = await Handler.Handle(request, context);

        return result.Match(
            response => Results.CreatedAtRoute("GetCampaignById", new { id = response.Id }, response),
            errors => errors.ToProblem());
    }
}