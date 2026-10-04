namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public static class ErrorCodes
{
    public const string InvalidCredentials = "auth/invalid-credentials";
    public const string OtpExpired = "auth/otp-expired";
    public const string TooManyAttempts = "auth/too-many-attempts";
}
