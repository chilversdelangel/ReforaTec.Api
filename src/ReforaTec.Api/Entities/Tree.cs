using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents an individual tree entity within an educational institution or campus.
/// </summary>
public class Tree : AuditableEntity
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    /// <summary>
    /// Historical planting date. Null if the exact planting date of an existing adult tree is unknown.
    /// </summary>
    public DateOnly? PlantingDate { get; set; }
    
    public int ValueId { get; set; }
    public Value? Value { get; set; }
    
    public int SpeciesId { get; set; }
    public Species? Species { get; set; }
    
    /// <summary>
    /// Latest measured height of the tree in centimeters. Null if not measured yet.
    /// </summary>
    public decimal? HeightCentimeters { get; set; }

    /// <summary>
    /// Latest measured trunk diameter of the tree in centimeters. Null if not measured yet.
    /// </summary>
    public decimal? DiameterCentimeters { get; set; }
    
    /// <summary>
    /// Geographic GPS coordinates and institution campus zone location of the tree.
    /// </summary>
    public Location? Location { get; set; }
    
    public string? Notes { get; set; }
    
    public TreeHealthState HealthState { get; set; } = TreeHealthState.Healthy;
}