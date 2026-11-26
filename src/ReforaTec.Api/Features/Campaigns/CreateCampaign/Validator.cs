using FluentValidation;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Code)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.Period)
            .NotNull();

        RuleFor(request => request.Period.StartDate)
            .NotEmpty();

        RuleFor(request => request.Period.EndDate)
            .GreaterThan(request => request.Period.StartDate)
            .When(request => request.Period.EndDate is not null)
            .WithMessage("'{PropertyName}' must be after start date.");
    }
}
