namespace ReforaTec.Api.Features.Common.Dtos;

public record PeriodDto(
    DateOnly? StartDate,
    DateOnly? EndDate
);