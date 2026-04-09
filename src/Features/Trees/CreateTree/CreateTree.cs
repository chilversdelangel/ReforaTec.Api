using FluentValidation;
using Mapster;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Filters;

namespace ReforaTec.Api.Features.Trees.CreateTree;

public static class CreateTree
{
    public record LocationDto(
        double? Latitude,
        double? Longitude,
        string Street,
        string Neighborhood,
        string StreetNumber
    );

    public record Request(
        DateOnly PlantingDate,
        int ValueId,
        int SpeciesId,
        decimal? Height,
        decimal? Diameter,
        LocationDto Location,
        string? Notes
    );

    public record Response(
        int Id,
        DateTime CreatedAt,
        DateTime ModifiedAt,
        DateOnly PlantingDate,
        int ValueId,
        int SpeciesId,
        decimal? Height,
        decimal? Diameter,
        LocationDto Location,
        string? Notes
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            RuleFor(x => x.PlantingDate)
                .InclusiveBetween(
                    today.AddDays(-7),
                    today.AddDays(7)
                )
                .WithMessage("The date should be within ±7 days from today");

            RuleFor(x => x.ValueId)
                .GreaterThan(0).WithMessage("ValueId must be a valid value");

            RuleFor(x => x.SpeciesId)
                .GreaterThan(0).WithMessage("SpeciesId must be a valid value");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Height must be a valid value");

            RuleFor(x => x.Diameter)
                .GreaterThan(0).WithMessage("Diameter must be a valid value");

            RuleFor(x => x.Location)
                .NotNull().WithMessage("Location is required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.Location.Neighborhood)
                        .NotEmpty().WithMessage("Neighborhood is required");

                    RuleFor(x => x.Location.StreetNumber)
                        .NotEmpty().WithMessage("StreetNumber is required");

                    RuleFor(x => x.Location.Street)
                        .NotEmpty().WithMessage("Street is required");
                });
        }
    }

    public static async Task<IResult> Handle(
        Request request,
        AppDbContext context
    )
    {
        var newTree = request.Adapt<Tree>();
        newTree.Status = TreeStatus.Planted;

        context.Trees.Add(newTree);
        await context.SaveChangesAsync();

        var treeResponse = newTree.Adapt<Response>();

        return Results.CreatedAtRoute("GetTreeById", new { id = treeResponse.Id }, treeResponse);
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/trees", Handle)
            .AddEndpointFilter<ValidationFilter<Request>>()
            .Produces<Response>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}