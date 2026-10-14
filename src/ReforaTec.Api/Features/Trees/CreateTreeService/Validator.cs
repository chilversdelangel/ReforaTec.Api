using FluentValidation;

namespace ReforaTec.Api.Features.Trees.CreateTreeService;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.ServiceTypeId)
            .NotEmpty();

        RuleFor(request => request.DeviceCapturedAt)
            .NotEmpty()
            .LessThanOrEqualTo(DateTimeOffset.UtcNow);

        RuleFor(request => request.Comment)
            .MaximumLength(500);
    }
}
