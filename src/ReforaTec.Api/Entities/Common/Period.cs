namespace ReforaTec.Api.Entities.Common;

public sealed record Period
{
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
