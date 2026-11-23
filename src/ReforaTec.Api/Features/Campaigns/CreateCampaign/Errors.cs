using ErrorOr;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to create campaigns.");

    public static Error CodeAlreadyExists(string code) => Error.Conflict(
        code: "campaign/code-already-exists",
        description: $"A campaign with code '{code}' already exists.");
}
