using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

internal sealed class RegisterUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", HandleRequest)
            .WithTags(OpenApiTags.Auth)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Register new user")
            .WithDescription("Registers a new student user under a verified institutional domain.");
    }

    private static async Task<IResult> HandleRequest(
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, context, cancellationToken);

        return result.Match(
            response => Results.Created($"/users/{response.Id}", response),
            errors => errors.ToProblem());
    }
}