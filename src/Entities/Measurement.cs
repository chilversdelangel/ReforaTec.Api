using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

public class Measurement : AuditableEntity
{
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }
    
    public int InspectorId { get; set; }
    public User? Inspector { get; set; }
    
    public int? CampaignId { get; set; }
    public Campaign? Campaign { get; set; }
    
    public MeasurementType InspectionType { get; set; } = MeasurementType.Baseline;
   
    public TreeHealthState DetectedHealthState { get; set; } = TreeHealthState.Healthy;
    
    public decimal HeightCentimeters { get; set; }
    public decimal DiameterMillimeters { get; set; }
    
    public string EvidencePhotoUrl { get; set; } = string.Empty;
    
    public DateTime DeviceCapturedAt { get; set; }
    public DateTime? ServerSyncedAt { get; set; }
    
    public string? ObservationNotes { get; set; }
}
