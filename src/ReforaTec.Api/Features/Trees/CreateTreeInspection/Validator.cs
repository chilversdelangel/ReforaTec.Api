using FluentValidation;

namespace ReforaTec.Api.Features.Trees.CreateTreeInspection;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.HealthState)
            .IsInEnum();

        RuleFor(request => request.Notes)
            .MaximumLength(500)
            .When(request => request.Notes is not null);

        RuleFor(request => request.DeviceCapturedAt)
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("'{PropertyName}' cannot be in the future.");
    }
}
