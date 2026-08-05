using FluentValidation;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.MiddleName));

        RuleFor(x => x.SecondLastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SecondLastName));

        RuleFor(x => x.ControlNumber)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
