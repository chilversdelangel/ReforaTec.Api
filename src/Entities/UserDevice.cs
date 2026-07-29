using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents a user's mobile device registration for Firebase Cloud Messaging (FCM) push notifications.
/// </summary>
public class UserDevice : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    /// <summary>
    /// Firebase Cloud Messaging (FCM) device push token.
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;
    
    public OperatingSystemType OsType { get; set; } = OperatingSystemType.Android;
    
    public bool IsActive { get; set; } = true;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
}
