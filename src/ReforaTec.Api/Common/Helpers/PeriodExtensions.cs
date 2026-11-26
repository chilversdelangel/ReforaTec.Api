using ReforaTec.Api.Entities.Common;

namespace ReforaTec.Api.Common.Helpers;

internal static class PeriodExtensions
{
    extension(Period period)
    {
        public bool IsActiveOn(DateOnly date) =>
            period.StartDate <= date && (date <= period.EndDate || period.EndDate is null);

        public bool HasEndedBefore(DateOnly date) =>
            period.EndDate < date;
    }
}
