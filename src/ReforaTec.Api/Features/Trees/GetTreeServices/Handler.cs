using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Trees.GetTreeServices;

internal static class Handler
{
    public static async Task<ErrorOr<List<Response>>> Handle(
        int treeId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var treeExists = await context.Trees
            .AnyAsync(t => t.Id == treeId, cancellationToken);

        if (!treeExists)
        {
            return Error.NotFound(
                code: ErrorCodes.NotFound,
                description: $"Tree with ID {treeId} was not found.");
        }

        var services = await context.Services
            .AsNoTracking()
            .Where(s => s.TreeId == treeId)
            .OrderByDescending(s => s.DeviceCapturedAt)
            .Select(s => new Response(
                s.Id,
                s.TreeId,
                s.ServiceTypeId,
                s.ServiceType!.ServiceName,
                s.StudentId,
                s.CampaignId,
                s.DeviceCapturedAt,
                s.CreatedAt,
                s.Comment))
            .ToListAsync(cancellationToken);

        return services;
    }
}
