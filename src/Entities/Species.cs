using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Species : AuditableEntity, INormalizable
{
    public string ScientificName { get; set; } = string.Empty;
    public string NormalizedScientificName { get; set; } = string.Empty;
    public List<string> CommonNames { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public void Normalize()
    {
        ScientificName = ScientificName.ToSanitized();
        NormalizedScientificName = ScientificName.ToNormalized();
    }
}