namespace ReforaTec.Api.Entities;

public class AuthOtpCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string VerificationCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}