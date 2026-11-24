using ErrorOr;

namespace ReforaTec.Api.Features.Campaigns.GetCampaigns;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to view campaigns.");
}
