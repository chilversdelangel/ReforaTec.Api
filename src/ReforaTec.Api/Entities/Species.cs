using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents a botanical tree species catalog entry.
/// </summary>
public class Species : AuditableEntity, INormalizable
{
    /// <summary>
    /// Latin botanical binomial name (e.g., "Quercus virginiana"). Globally unique per species.
    /// </summary>
    public string ScientificName { get; set; } = string.Empty;
    public string NormalizedScientificName { get; set; } = string.Empty;
    
    /// <summary>
    /// Regional common name (e.g., "Encino Siempreverde").
    /// Non-unique as multiple Mexican tree species may share common names.
    /// </summary>
    public string CommonName { get; set; } = string.Empty;
    public string NormalizedCommonName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Relative or absolute URL for the species catalog image displayed in the mobile app.
    /// </summary>
    public string? ImageUrl { get; set; }

    public void Normalize()
    {
        ScientificName = ScientificName.ToSanitized();
        NormalizedScientificName = ScientificName.ToNormalized();

        CommonName = CommonName.ToSanitized();
        NormalizedCommonName = CommonName.ToNormalized();
    }
}