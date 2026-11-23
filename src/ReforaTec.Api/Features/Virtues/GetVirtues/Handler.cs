using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Virtues.GetVirtues;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var virtues = await context.Virtues
            .AsNoTracking()
            .OrderBy(virtue => virtue.Name)
            .Select(virtue => new VirtueDto(
                virtue.Id,
                virtue.Name))
            .ToListAsync(cancellationToken);

        return new Response(virtues);
    }
}
