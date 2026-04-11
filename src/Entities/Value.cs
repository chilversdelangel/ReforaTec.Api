using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Value : AuditableEntity
{
    public string ValueName { get; set; }
}