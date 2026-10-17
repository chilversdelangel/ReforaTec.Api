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
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? SecondLastName { get; set; }
    public string? DisplayName { get; set; }
    
    public string? PhoneNumber { get; set; }
    public Gender Gender { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public void Normalize()
    {
        Email = Email.ToNormalized();

        ControlNumber = ControlNumber?.ToSanitized().ToUpperInvariant();

        FirstName = FirstName.ToSanitized();
        LastName = LastName.ToSanitized();
        SecondLastName = SecondLastName?.ToSanitizedOrNull();
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
