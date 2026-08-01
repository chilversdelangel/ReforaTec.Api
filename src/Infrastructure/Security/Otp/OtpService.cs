using System.Security.Cryptography;

namespace ReforaTec.Api.Infrastructure.Security.Otp;

public class OtpService : IOtpService
{
    public string GenerateOtp(int length)
    {
        // Limit to >= 4 for basic security.
        // Limit to <= 9 to prevent Integer Overflow when calculating Math.Pow (10^10 > int.MaxValue).
        if (length is < 4 or > 9)
            throw new ArgumentOutOfRangeException(nameof(length), "OTP length must be between 4 and 9.");

        var maxExclusive = (int)Math.Pow(10, length);
        
        // Cryptographically secure generation avoiding Modulo Bias
        var otp = RandomNumberGenerator.GetInt32(0, maxExclusive);
        
        // Pad with leading zeros based on length (e.g. D6)
        return otp.ToString($"D{length}");
    }
}
