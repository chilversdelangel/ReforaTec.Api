using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.ServiceTypes.UploadServiceTypeIcon;

internal sealed class UploadServiceTypeIcon : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/service-types/icons", HandleRequest)
            .DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Upload service type icon")
            .WithDescription("Uploads an icon image (PNG/WebP/JPEG, max 1MB) for a service type catalog entry.")
            .RequireAuthorization();
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
