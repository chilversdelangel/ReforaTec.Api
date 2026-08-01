namespace ReforaTec.Api.Infrastructure.Security.Otp;

public interface IOtpService
{
    /// <summary>
    /// Generates a cryptographically secure numeric OTP of the specified length.
    /// </summary>
    string GenerateOtp(int length = 6);
}
