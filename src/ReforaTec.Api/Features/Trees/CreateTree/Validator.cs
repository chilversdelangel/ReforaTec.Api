using FluentValidation;

namespace ReforaTec.Api.Features.Trees.CreateTree;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Folio)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(request => request.SpeciesId)
            .GreaterThan(0)
            .WithMessage("'{PropertyName}' must be valid.");

        RuleFor(request => request.VirtueId)
            .GreaterThan(0)
            .WithMessage("'{PropertyName}' must be valid.");

        RuleFor(request => request.PlantingDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .When(request => request.PlantingDate is not null)
            .WithMessage("'{PropertyName}' cannot be in the future.");

        When(request => request.Location is not null, () =>
        {
            RuleFor(request => request.Location!.Latitude)
                .NotNull()
                .InclusiveBetween(-90.0, 90.0);

            RuleFor(request => request.Location!.Longitude)
                .NotNull()
                .InclusiveBetween(-180.0, 180.0);
        });
    }
}
