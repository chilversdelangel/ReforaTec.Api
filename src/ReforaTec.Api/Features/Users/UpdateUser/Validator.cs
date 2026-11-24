using FluentValidation;

namespace ReforaTec.Api.Features.Users.UpdateUser;

internal sealed class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.Role)
            .IsInEnum()
            .WithMessage("'{PropertyName}' must be a valid role.")
            .When(request => request.Role.HasValue);
    }
}
