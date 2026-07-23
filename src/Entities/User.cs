using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;
using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Entities;

public class User : AuditableEntity, INormalizable
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public UserRole CurrentRole { get; set; } = UserRole.Student;
    
    public string InstitutionalEmail { get; set; } = string.Empty;
    public string NormalizedInstitutionalEmail { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public void Normalize()
    {
        InstitutionalEmail = InstitutionalEmail.ToSanitized();
        NormalizedInstitutionalEmail = InstitutionalEmail.ToNormalized();
        
        FirstName = FirstName.ToSanitized();
        LastName = LastName.ToSanitized();
        
        if (MiddleName is not null) MiddleName = MiddleName.ToSanitized();
        if (SecondLastName is not null) SecondLastName = SecondLastName.ToSanitized();
    }
}
