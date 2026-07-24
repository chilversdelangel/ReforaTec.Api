using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Tenant : AuditableEntity, INormalizable
{
    public string InstitutionName { get; set; } = string.Empty;
    public string NormalizedInstitutionName { get; set; } = string.Empty;
    
    public string InstitutionalEmailDomain { get; set; } = string.Empty;
    public string NormalizedInstitutionalEmailDomain { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public void Normalize()
    {
        InstitutionName = InstitutionName.ToSanitized();
        NormalizedInstitutionName = InstitutionName.ToNormalized();
        
        InstitutionalEmailDomain = InstitutionalEmailDomain.ToSanitized();
        NormalizedInstitutionalEmailDomain = InstitutionalEmailDomain.ToNormalized();
    }
}
