using Microsoft.EntityFrameworkCore;

namespace ReforaTec.Api.Entities.Common;

[Owned]
public record Period
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}