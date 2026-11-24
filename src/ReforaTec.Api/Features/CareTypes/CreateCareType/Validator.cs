using FluentValidation;

namespace ReforaTec.Api.Features.CareTypes.CreateCareType;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.IconUrl)
            .NotEmpty()
            .MaximumLength(300);
    }
}
