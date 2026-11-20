using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.CareTypes.GetCareTypes;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var careTypes = await context.CareTypes
            .AsNoTracking()
            .Where(careType => careType.IsActive)
            .OrderBy(careType => careType.Id)
            .Select(careType => new CareTypeDto(
                careType.Id,
                careType.Name,
                careType.IconUrl))
            .ToListAsync(cancellationToken);

        return new Response(careTypes);
    }
}
