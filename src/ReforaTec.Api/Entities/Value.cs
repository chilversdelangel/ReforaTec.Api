using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Value : AuditableEntity, INormalizable
{
    public string ValueName { get; set; } = string.Empty;
    public string NormalizedValueName { get; set; } = string.Empty;

    public void Normalize() => NormalizedValueName = ValueName.ToNormalized();
}