using FluentValidation;

namespace ReforaTec.Api.Features.CareAssignments.CloseCareAssignment;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.EndDate)
            .NotEmpty();
    }
}
