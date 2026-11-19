using FluentValidation;

namespace ReforaTec.Api.Features.Trees.CreateTreeMeasurement;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.HeightCentimeters)
            .GreaterThan(0);

        RuleFor(request => request.DeviceCapturedAt)
            .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow.AddMinutes(5))
            .WithMessage("'{PropertyName}' cannot be in the future.");
    }
}
