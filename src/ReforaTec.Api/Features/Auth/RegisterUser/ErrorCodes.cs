namespace ReforaTec.Api.Features.Auth.RegisterUser;

public static class ErrorCodes
{
    public const string EmailAlreadyExists = "user/email-already-exists";
    public const string ControlNumberAlreadyExists = "user/control-number-already-exists";
    public const string InstitutionalDomainNotFound = "tenant/institutional-domain-not-found";
}
