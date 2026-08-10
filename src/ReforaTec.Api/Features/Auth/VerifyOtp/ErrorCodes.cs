namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public static class ErrorCodes
{
    public const string InvalidCredentials = "Auth.InvalidCredentials";
    public const string OtpExpired = "Auth.OtpExpired";
    public const string TooManyAttempts = "Auth.TooManyAttempts";
}
