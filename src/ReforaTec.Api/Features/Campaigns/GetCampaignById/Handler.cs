using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

public static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(int id, AppDbContext context)
    {
        if (id <= 0)
            return Error.Validation(
                code: ErrorCodes.InvalidId,
                description: $"Id {id} is invalid."
            );

        var campaign = await context.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == id)
            .ProjectToType<Response>()
            .FirstOrDefaultAsync();

        if (campaign == null)
            return Error.NotFound(
                code: ErrorCodes.NotFound,
                description: $"Campaign with id {id} not found."
            );

        return campaign;
    }
}