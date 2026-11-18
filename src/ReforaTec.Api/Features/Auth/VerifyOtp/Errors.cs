using ErrorOr;

namespace ReforaTec.Api.Features.Auth.VerifyOtp;

internal static class Errors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        code: "auth/invalid-credentials",
        description: "Invalid email or OTP code.");

    public static readonly Error OtpExpired = Error.Unauthorized(
        code: "auth/otp-expired",
        description: "OTP code has expired. Please request a new one.");

    public static readonly Error TooManyAttempts = Error.Unauthorized(
        code: "auth/otp-too-many-attempts",
        description: "Too many failed attempts. Please request a new OTP code.");

    public static readonly Error TooManyAttemptsDestroyed = Error.Unauthorized(
        code: "auth/otp-too-many-attempts",
        description: "Too many failed attempts. OTP code destroyed.");
}
