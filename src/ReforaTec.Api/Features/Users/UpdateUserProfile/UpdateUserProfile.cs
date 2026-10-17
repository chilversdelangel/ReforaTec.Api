using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed class UpdateUserProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/me", HandleRequest)
            .WithName("UpdateUserProfile")
            .WithTags(OpenApiTags.Users)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Partially update authenticated user profile")
            .WithDescription("Partially updates personal information for the currently authenticated user. Only provided fields are updated; omitted fields retain their existing values. To clear an optional field (secondLastName, displayName, phoneNumber), pass an empty string \"\".");
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

        var result = await Handler.Handle(
            request,
            identityResult.Value,
            context,
            cancellationToken);

        return result.Match(
            Results.Ok,
            errors => errors.ToProblem());
    }
}
