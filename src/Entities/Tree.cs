namespace ReforaTec.Api.Entities;

public class Tree
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public DateOnly? PlantingDate { get; set; }
    public int ValueId { get; set; }
    public int SpeciesId { get; set; }
    public decimal? Height { get; set; }
    public decimal? Diameter { get; set; }
    public Location Location { get; set; }
    public string? Notes { get; set; }
}