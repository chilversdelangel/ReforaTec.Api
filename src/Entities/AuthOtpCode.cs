namespace ReforaTec.Api.Entities;

public class AuthOtpCode
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public string VerificationCode { get; set; } = string.Empty;
    public int FailedAttempts { get; set; } = 0;
    
    public DateTime ExpiresAt { get; set; }
}