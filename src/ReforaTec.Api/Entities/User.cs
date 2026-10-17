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
    
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public string? ControlNumber { get; set; }
    
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string? SecondFamilyName { get; set; }
    public string? DisplayName { get; set; }
    
    public string? PhoneNumber { get; set; }
    public Gender Gender { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public void Normalize()
    {
        Email = Email.ToNormalized();

        ControlNumber = ControlNumber?.ToSanitized().ToUpperInvariant();

        GivenName = GivenName.ToSanitized();
        FamilyName = FamilyName.ToSanitized();
        SecondFamilyName = SecondFamilyName?.ToSanitizedOrNull();
        DisplayName = DisplayName?.ToSanitizedOrNull();

        NormalizePhoneNumber();
    }

    private void NormalizePhoneNumber()
    {
        var phoneNumberSanitized = PhoneNumber?.Trim();
        if (string.IsNullOrEmpty(phoneNumberSanitized))
        {
            PhoneNumber = null;
            return;
        }

        PhoneNumber = phoneNumberSanitized.StartsWith('+')
            ? phoneNumberSanitized
            : $"+52{phoneNumberSanitized}";
    }
}
