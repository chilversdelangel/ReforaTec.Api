using ErrorOr;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

public static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(Request request, AppDbContext context)
    {
        var normalizedName = request.CampaignName.ToNormalized();

        var exist = await context.Campaigns
            .AnyAsync(campaign => campaign.NormalizedCampaignName == normalizedName);

        if (exist)
            return Error.Conflict(
                code: ErrorCodes.Duplicate,
                description: "Campaign already exists"
            );

        var newCampaign = request.Adapt<Campaign>();

        context.Campaigns.Add(newCampaign);
        await context.SaveChangesAsync();

        var campaignResponse = newCampaign.Adapt<Response>();

        return campaignResponse;
    }
}