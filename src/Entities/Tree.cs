using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

public class Tree : AuditableEntity
{
    public DateOnly? PlantingDate { get; set; }
    public int ValueId { get; set; }
    public int SpeciesId { get; set; }
    public decimal? Height { get; set; }
    public decimal? Diameter { get; set; }
    public Location Location { get; set; }
    public string? Notes { get; set; }
    public TreeStatus Status { get; set; }
}