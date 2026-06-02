using ErrorOr;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Infrastructure.Filters;

namespace ReforaTec.Api.Features.Species.CreateSpecies;

public static class CreateSpecies
{
    public record Request(
        string ScientificName,
        List<string> CommonNames,
        string Description,
        string? ImageUrl
    );

    public record Response(
        int Id,
        string ScientificName,
        List<string> CommonNames,
        string Description,
        string? ImageUrl,
        DateTime CreatedAt,
        DateTime ModifiedAt
    );

    private static class ErrorCodes
    {
        public const string NameRequired = "Species.ScientificName.Required";
        public const string DescriptionRequired = "Species.Description.Required";
        public const string Duplicate = "Species.Duplicate";
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.ScientificName)
                .NotEmpty().WithErrorCode(ErrorCodes.NameRequired)
                .MaximumLength(100);

            RuleFor(x => x.CommonNames)
                .NotNull();

            RuleForEach(x => x.CommonNames)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .NotEmpty().WithErrorCode(ErrorCodes.DescriptionRequired)
                .MaximumLength(1000);

            RuleFor(x => x.ImageUrl)
                .MaximumLength(2048);
        }
    }

    public static async Task<ErrorOr<Response>> Handle(Request request, AppDbContext context)
    {
        var normalizedName = request.ScientificName.ToNormalized();
        var exists = await context.Species
            .AnyAsync(s => s.NormalizedScientificName == normalizedName);

        if (exists)
            return Error.Conflict(code: ErrorCodes.Duplicate, description: "Species already exists");

        var newSpecies = request.Adapt<Entities.Species>();

        context.Species.Add(newSpecies);
        await context.SaveChangesAsync();

        var speciesResponse = newSpecies.Adapt<Response>();

        return speciesResponse;
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/species", async (Request request, AppDbContext context) =>
            {
                var result = await Handle(request, context);

                return result.Match(
                    response => Results.CreatedAtRoute("GetSpeciesById", new { id = response.Id }, response),
                    errors =>
                    {
                        return errors.Any(e => e.Type == ErrorType.Conflict)
                            ? Results.Conflict(new { detail = errors[0].Description })
                            : Results.Problem(statusCode: 500, title: "Internal Server Error");
                    }
                );
            })
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}