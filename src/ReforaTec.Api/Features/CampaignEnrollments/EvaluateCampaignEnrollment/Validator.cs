using FluentValidation;

namespace ReforaTec.Api.Features.CampaignEnrollments.EvaluateCampaignEnrollment;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.EvaluationNotes)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.EvaluationNotes));
    }
}
