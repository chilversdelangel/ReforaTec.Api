using Microsoft.EntityFrameworkCore;

namespace ReforaTec.Api.Entities.Common;

[Owned]
public record Period
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}