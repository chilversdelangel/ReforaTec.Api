using FluentValidation;

namespace ReforaTec.Api.Features.CampaignEnrollments.EnrollInCampaign;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.CampaignCode)
            .NotEmpty()
            .MaximumLength(20);
    }
}
