using ErrorOr;

namespace ReforaTec.Api.Features.Campaigns.CreateCareAssignment;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to assign trees in a campaign.");

    public static Error CampaignNotFound(int campaignId) => Error.NotFound(
        code: "campaign/not-found",
        description: $"Campaign with ID {campaignId} was not found.");

    public static readonly Error CampaignNotActive = Error.Conflict(
        code: "campaign/not-active",
        description: "Cannot assign trees to a campaign that is not currently active.");

    public static Error EnrollmentNotFound(int userId, int campaignId) => Error.NotFound(
        code: "campaign-enrollment/not-found",
        description: $"User with ID {userId} is not enrolled in campaign {campaignId}.");

    public static Error TreeNotFound(int treeId) => Error.NotFound(
        code: "tree/not-found",
        description: $"Tree with ID {treeId} was not found.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        code: "care-assignment/already-assigned",
        description: "This user already has an active care assignment for this tree in the campaign.");
}
