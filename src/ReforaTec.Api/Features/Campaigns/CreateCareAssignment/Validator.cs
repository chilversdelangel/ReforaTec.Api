using FluentValidation;

namespace ReforaTec.Api.Features.Campaigns.CreateCareAssignment;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.UserId)
            .GreaterThan(0)
            .WithMessage("'{PropertyName}' must be valid.");

        RuleFor(request => request.TreeId)
            .GreaterThan(0)
            .WithMessage("'{PropertyName}' must be valid.");

        RuleFor(request => request.StartDate)
            .NotEmpty();
    }
}
