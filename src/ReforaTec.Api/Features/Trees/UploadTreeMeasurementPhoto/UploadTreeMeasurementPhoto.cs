using Microsoft.AspNetCore.Mvc;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Endpoints;
using ReforaTec.Api.Infrastructure.Filters;
using ReforaTec.Api.Infrastructure.Mapping;
using ReforaTec.Api.Infrastructure.Security.Authorization;
using ReforaTec.Api.Infrastructure.Storage;

namespace ReforaTec.Api.Features.Trees.UploadTreeMeasurementPhoto;

internal sealed class UploadTreeMeasurementPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/trees/{treeId:int}/measurements/photo", HandleRequest)
            .DisableAntiforgery()
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Upload tree measurement photo")
            .WithDescription("Uploads an evidence photo (PNG/WebP/JPEG, max 5MB) for a tree measurement.")
            .RequireAuthorization(Policy.CanRecordMeasurements);
    }

    private static async Task<IResult> HandleRequest(
        int treeId,
        [FromForm] Request request,
        AppDbContext dbContext,
        IFileStorageService storageService,
        CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(
            treeId, 
            request, 
            dbContext, 
            storageService, 
            cancellationToken);

        return result.Match(
            response => Results.Created(response.FileUrl, response),
            errors => errors.ToProblem());
    }
}
