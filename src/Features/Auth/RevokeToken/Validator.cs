using FluentValidation;

namespace ReforaTec.Api.Features.Auth.RevokeToken;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
