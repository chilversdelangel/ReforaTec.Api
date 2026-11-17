using FluentValidation;

namespace ReforaTec.Api.Features.Trees.CreateTreeCareLog;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.CareTypeId)
            .NotEmpty();

        RuleFor(request => request.PerformedAt)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow);

        RuleFor(request => request.DeviceCapturedAt)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow);

        RuleFor(request => request.Notes)
            .MaximumLength(500);
    }
}
