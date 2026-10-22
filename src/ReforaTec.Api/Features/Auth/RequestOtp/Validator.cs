using FluentValidation;

namespace ReforaTec.Api.Features.Auth.RequestOtp;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
