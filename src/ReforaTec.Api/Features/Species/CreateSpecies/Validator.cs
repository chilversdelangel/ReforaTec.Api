using FluentValidation;

namespace ReforaTec.Api.Features.Species.CreateSpecies;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.ScientificName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.CommonName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(request => request.ImageUrl)
            .NotEmpty()
            .MaximumLength(2048);
    }
}
