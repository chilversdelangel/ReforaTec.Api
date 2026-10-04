using FluentValidation;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RefreshSession;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.Audience)
            .NotEmpty()
            .Must(a => a is Audience.StudentMobileApp or Audience.InspectorMobileApp or Audience.WebDashboard)
            .WithMessage("The requested audience is not authorized.");
    }
}
