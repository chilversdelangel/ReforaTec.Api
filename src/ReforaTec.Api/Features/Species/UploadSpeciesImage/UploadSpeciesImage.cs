using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.OpenApi;
using ReforaTec.Api.Infrastructure.Security.Authorization;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.Species.UploadSpeciesImage;

internal sealed class UploadSpeciesImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/species/images", HandleRequest)
            .WithTags(OpenApiTags.Catalogs)
            .DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Upload species image")
            .WithDescription("Uploads a representative photo (PNG/WebP/JPEG, max 5MB) for a botanical species.")
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
