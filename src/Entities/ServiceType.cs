using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class ServiceType : AuditableEntity, INormalizable
{
    public string ServiceName { get; set; } = string.Empty;
    public string NormalizedServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Relative or absolute URL for the service icon displayed in the mobile app grid.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Flag indicating whether this service type is active and selectable in the mobile app.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public void Normalize()
    {
        ServiceName = ServiceName.ToSanitized();
        NormalizedServiceName = ServiceName.ToNormalized();
    }
}
