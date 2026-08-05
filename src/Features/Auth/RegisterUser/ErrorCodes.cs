namespace ReforaTec.Api.Features.Auth.RegisterUser;

public static class ErrorCodes
{
    public const string EmailAlreadyExists = "User.EmailAlreadyExists";
    public const string ControlNumberAlreadyExists = "User.ControlNumberAlreadyExists";
    public const string InstitutionalDomainNotFound = "Tenant.InstitutionalDomainNotFound";
}
