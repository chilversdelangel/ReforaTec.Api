using FluentValidation;

namespace ReforaTec.Api.Features.Auth.RevokeSession;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
