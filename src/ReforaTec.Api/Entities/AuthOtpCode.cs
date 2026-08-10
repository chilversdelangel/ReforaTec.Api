namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents a single-use temporary 6-digit One-Time Password (OTP) verification code.
/// Uses Shared Primary Key pattern where UserId is both Primary Key and Foreign Key,
/// enforcing strictly 1 active OTP per user. Consumed OTPs are hard-deleted upon verification.
/// </summary>
public class AuthOtpCode
{
    public const int MaxFailedAttempts = 3;

    public int UserId { get; set; }
    public User? User { get; set; }
    
    public string VerificationCode { get; set; } = string.Empty;
    public int FailedAttempts { get; set; } = 0;
    
    public DateTime ExpiresAt { get; set; }
}