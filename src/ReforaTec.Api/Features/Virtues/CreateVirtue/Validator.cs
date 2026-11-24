using FluentValidation;

namespace ReforaTec.Api.Features.Virtues.CreateVirtue;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(25);
    }
}
