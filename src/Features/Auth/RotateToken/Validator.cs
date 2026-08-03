using FluentValidation;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.RotateToken;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.Audience)
            .NotEmpty()
            .Must(a => a is Audience.MobileApp or Audience.AdminDashboard)
            .WithMessage("The requested audience is not authorized.");
    }
}
