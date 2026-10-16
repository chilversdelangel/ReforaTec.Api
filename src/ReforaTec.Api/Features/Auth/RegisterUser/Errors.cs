using ErrorOr;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

internal static class Errors
{
    public static readonly Error EmailAlreadyExists = Error.Conflict(
        code: "user/email-already-exists",
        description: "A user with this email already exists.");

    public static readonly Error ControlNumberAlreadyExists = Error.Conflict(
        code: "user/control-number-already-exists",
        description: "A user with this control number already exists in the institution.");

    public static readonly Error InstitutionalDomainNotFound = Error.NotFound(
        code: "tenant/institutional-domain-not-found",
        description: "The provided email domain does not belong to any registered institution.");
}
