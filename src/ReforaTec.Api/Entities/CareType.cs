using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public sealed class CareType : AuditableEntity, INormalizable
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Relative or absolute URL for the care type icon displayed in the mobile app grid.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// Flag indicating whether this care type is active and selectable in the mobile app.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public void Normalize()
    {
        Name = Name.ToSanitized();
        NormalizedName = Name.ToNormalized();
    }
}
