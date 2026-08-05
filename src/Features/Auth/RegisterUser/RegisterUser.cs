using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

public static class RegisterUser
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", HandleRequest)
            .AllowAnonymous()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound);
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
