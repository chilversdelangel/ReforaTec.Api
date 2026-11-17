using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Common.Interfaces;
using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Virtue : AuditableEntity, INormalizable
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;

    public void Normalize()
    {
        Name = Name.ToSanitized();
        NormalizedName = Name.ToNormalized();
    }
}
