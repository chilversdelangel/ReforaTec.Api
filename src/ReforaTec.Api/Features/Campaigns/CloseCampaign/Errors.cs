using ErrorOr;

namespace ReforaTec.Api.Features.Campaigns.CloseCampaign;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to close campaigns.");

    public static Error CampaignNotFound(int campaignId) => Error.NotFound(
        code: "campaign/not-found",
        description: $"Campaign with ID {campaignId} was not found.");

    public static readonly Error AlreadyClosed = Error.Conflict(
        code: "campaign/already-closed",
        description: "This campaign is already closed and has no active care assignments.");
}
