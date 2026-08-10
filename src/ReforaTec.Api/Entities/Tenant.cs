using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Tenant : AuditableEntity, INormalizable
{
    public string InstitutionName { get; set; } = string.Empty;
    public string NormalizedInstitutionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Institutional acronym used for campaign inscription code generation (e.g. "ITCM", "ITT", "UNAM").
    /// </summary>
    public string Acronym { get; set; } = string.Empty;
    
    /// <summary>
    /// Institutional email domain used for automatic student tenant assignment (e.g. "cdmadero.tecnm.mx").
    /// </summary>
    public string InstitutionalEmailDomain { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public void Normalize()
    {
        InstitutionName = InstitutionName.ToSanitized();
        NormalizedInstitutionName = InstitutionName.ToNormalized();
        
        Acronym = Acronym.Trim().ToUpperInvariant();
        InstitutionalEmailDomain = InstitutionalEmailDomain.ToNormalized();
    }
}
