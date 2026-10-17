using FluentValidation;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(request => request.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .When(request => request.FirstName is not null);

        RuleFor(request => request.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .When(request => request.LastName is not null);

        RuleFor(request => request.SecondLastName)
            .MaximumLength(50)
            .When(request => !string.IsNullOrEmpty(request.SecondLastName));

        RuleFor(request => request.DisplayName)
            .MaximumLength(50)
            .When(request => !string.IsNullOrEmpty(request.DisplayName));

        RuleFor(request => request.PhoneNumber)
            .Matches(@"^(\+[1-9]\d{9,14}|\d{10})$")
            .When(request => !string.IsNullOrWhiteSpace(request.PhoneNumber));

        RuleFor(request => request.Gender)
            .IsInEnum()
            .WithMessage("Gender must be a valid value: 0 (Male), 1 (Female), or 2 (Other).")
            .When(request => request.Gender.HasValue);
    }

    private static bool HaveAtLeastOneField(Request request) =>
        request.FirstName is not null ||
        request.LastName is not null ||
        request.SecondLastName is not null ||
        request.DisplayName is not null ||
        request.PhoneNumber is not null ||
        request.Gender.HasValue;
}
