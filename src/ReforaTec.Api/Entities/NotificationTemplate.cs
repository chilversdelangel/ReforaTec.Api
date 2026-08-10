using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents automated global notification templates triggered by tree health state changes or reminders.
/// </summary>
public class NotificationTemplate : AuditableEntity
{
    public NotificationType Type { get; set; } = NotificationType.TreeHealthAlert;
    
    /// <summary>
    /// Optional target health state. When specified, this template triggers automatically 
    /// when an inspector detects this specific health state (e.g. NeedsAttention, Critical, Dead).
    /// </summary>
    public TreeHealthState? TargetHealthState { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}
