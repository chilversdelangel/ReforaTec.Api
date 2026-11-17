using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Authorization;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.CareTypes.UploadCareTypeIcon;

internal sealed class UploadCareTypeIcon : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/care-types/icons", HandleRequest)
            .WithName("UploadCareTypeIcon")
            .WithTags(OpenApiTags.Catalogs)
            .DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithSummary("Upload care type icon")
            .WithDescription("Uploads an icon image (PNG/WebP/JPEG, max 1MB) for a care type catalog entry.")
            .RequireAuthorization(Policy.CanManageCatalogs);
    }

    private static async Task<IResult> HandleRequest(
        [FromForm] Request request,
        IFileStorageService storageService,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, storageService, cancellationToken);

        return result.Match(
            response => Results.Created(response.FileUrl, response),
            errors => errors.ToProblem());
    }
}
