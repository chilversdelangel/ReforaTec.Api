using ErrorOr;

namespace ReforaTec.Api.Features.CampaignEnrollments.EnrollInCampaign;

internal static class Errors
{
    public static Error CampaignNotFound(string campaignCode) => Error.NotFound(
        code: "campaign/not-found",
        description: $"No campaign with code '{campaignCode}' was found.");

    public static readonly Error CampaignNotActive = Error.Conflict(
        code: "campaign/not-active",
        description: "This campaign is not currently active for enrollment.");

    public static readonly Error AlreadyEnrolled = Error.Conflict(
        code: "campaign-enrollment/already-enrolled",
        description: "You are already enrolled in this campaign.");
}
