using FluentValidation;

namespace ReforaTec.Api.Features.Auth.RequestOtp;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
