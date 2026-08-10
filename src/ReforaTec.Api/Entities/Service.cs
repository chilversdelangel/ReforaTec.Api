using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Service : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int StudentId { get; set; }
    public User? Student { get; set; }
    
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }
    
    public int ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }
    
    /// <summary>
    /// Optional foreign key to Campaign. Null indicates the service was performed 
    /// on a tree outside a formal academic campaign.
    /// </summary>
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    /// <summary>
    /// Timestamp captured on the mobile device when the service was logged offline.
    /// </summary>
    public DateTime DeviceCapturedAt { get; set; }

    /// <summary>
    /// Timestamp when the mobile record was synchronized to the backend server.
    /// </summary>
    public DateTime? ServerSyncedAt { get; set; }

    public string? Comment { get; set; }
}
