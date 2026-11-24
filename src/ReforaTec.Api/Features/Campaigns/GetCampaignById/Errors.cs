using ErrorOr;

namespace ReforaTec.Api.Features.Campaigns.GetCampaignById;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to view this campaign.");

    public static Error CampaignNotFound(int campaignId) => Error.NotFound(
        code: "campaign/not-found",
        description: $"Campaign with ID {campaignId} was not found.");
}
