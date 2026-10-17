using FluentValidation;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(request => request.GivenName)
            .NotEmpty()
            .MaximumLength(100)
            .When(request => request.GivenName is not null);

        RuleFor(request => request.FamilyName)
            .NotEmpty()
            .MaximumLength(50)
            .When(request => request.FamilyName is not null);

        RuleFor(request => request.SecondFamilyName)
            .MaximumLength(50)
            .When(request => !string.IsNullOrEmpty(request.SecondFamilyName));

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
        request.GivenName is not null ||
        request.FamilyName is not null ||
        request.SecondFamilyName is not null ||
        request.DisplayName is not null ||
        request.PhoneNumber is not null ||
        request.Gender.HasValue;
}
