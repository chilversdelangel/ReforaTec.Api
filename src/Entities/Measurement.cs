using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

public class Measurement : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }
    
    public int InspectorId { get; set; }
    public User? Inspector { get; set; }
    
    /// <summary>
    /// Optional foreign key to Campaign. Null indicates the measurement was performed 
    /// on a tree outside a formal academic campaign.
    /// </summary>
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public MeasurementType InspectionType { get; set; } = MeasurementType.Baseline;
   
    public TreeHealthState DetectedHealthState { get; set; } = TreeHealthState.Healthy;
    
    public decimal HeightCentimeters { get; set; }
    public decimal DiameterCentimeters { get; set; }
    
    /// <summary>
    /// Optional photo URL of the tree inspection evidence (typically captured 
    /// during baseline and final delivery inspections).
    /// </summary>
    public string? EvidencePhotoUrl { get; set; }
    
    /// <summary>
    /// Timestamp captured on the mobile device when the measurement was logged offline.
    /// </summary>
    public DateTime DeviceCapturedAt { get; set; }

    /// <summary>
    /// Timestamp when the mobile record was synchronized to the backend server.
    /// </summary>
    public DateTime? ServerSyncedAt { get; set; }
    
    public string? ObservationNotes { get; set; }
}
