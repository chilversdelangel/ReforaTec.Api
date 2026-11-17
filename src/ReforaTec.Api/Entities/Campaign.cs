using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

/// <summary>
/// Represents an institutional environmental campaign within an educational institution.
/// </summary>
public sealed class Campaign : AuditableEntity, INormalizable
{
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public required Period Period { get; set; }

    /// <summary>
    /// Unique human-readable campaign identifier and enrollment code (e.g. "ITCM-2026-A").
    /// </summary>
    public string Code { get; set; } = string.Empty;
    public string NormalizedCode { get; set; } = string.Empty;

    public void Normalize()
    {
        Code = Code.ToSanitized().ToUpperInvariant();
        NormalizedCode = Code.ToNormalized();
    }
}
