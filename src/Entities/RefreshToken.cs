using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class RefreshToken : CreatableEntity
{
    public string HashedToken { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    public string Audience { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}
