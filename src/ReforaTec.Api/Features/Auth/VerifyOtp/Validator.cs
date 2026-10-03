using FluentValidation;
using ReforaTec.Api.Infrastructure.Security.Jwt;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .Length(6)
            .Matches(@"^\d+$").WithMessage("OTP code must contain only digits.");

        RuleFor(x => x.Audience)
            .NotEmpty()
            .Must(a => a is Audience.MobileApp or Audience.AdminDashboard)
            .WithMessage("The requested audience is not authorized.");
    }
}
