using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        int id, 
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var campaign = await context.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == id)
            .ProjectToType<Response>()
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign is null)
        {
            return Error.NotFound(
                code: ErrorCodes.NotFound,
                description: $"Campaign with ID {id} was not found.");
        }

        return campaign;
    }
}