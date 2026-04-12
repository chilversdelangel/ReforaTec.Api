using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Species : AuditableEntity
{
    public string ScientificName { get; set; } = string.Empty;
    public List<string> CommonNames { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}