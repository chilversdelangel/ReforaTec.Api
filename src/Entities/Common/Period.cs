namespace ReforaTec.Api.Entities.Common;

public record Period
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}