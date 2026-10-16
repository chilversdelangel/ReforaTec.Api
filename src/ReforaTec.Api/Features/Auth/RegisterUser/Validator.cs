using FluentValidation;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.MiddleName)
            .MaximumLength(50);

        RuleFor(request => request.SecondLastName)
            .MaximumLength(50);

        RuleFor(request => request.ControlNumber)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(request => request.PhoneNumber)
            .MaximumLength(20);

        RuleFor(request => request.Gender)
            .IsInEnum()
            .WithMessage("Gender must be a valid value: 0 (Male), 1 (Female), or 2 (Other).");
    }
}
