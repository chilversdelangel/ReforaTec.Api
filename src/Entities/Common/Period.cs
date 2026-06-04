namespace ReforaTec.Api.Entities.Common;

public record Period
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}