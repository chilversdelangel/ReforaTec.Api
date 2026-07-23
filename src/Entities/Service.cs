using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Entities;

public class Service : AuditableEntity
{
    public int StudentId { get; set; }
    public User? Student { get; set; }
    public int TreeId { get; set; }
    public Tree? Tree { get; set; }
    public int ServiceTypeId { get; set; }
    public ServiceType? ServiceType { get; set; }
    public DateTime DeviceCapturedAt { get; set; }
    public DateTime? ServerSyncedAt { get; set; }
    public string? Comment { get; set; }
}
