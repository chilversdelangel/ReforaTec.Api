using ErrorOr;

namespace ReforaTec.Api.Features.Trees.CreateTreeCareLog;

internal static class Errors
{
    public static Error TreeNotFound(int treeId) => Error.NotFound(
        code: "tree/not-found",
        description: $"Tree with ID {treeId} was not found.");

    public static readonly Error UserNotAssignedToTree = Error.Forbidden(
        code: "user/not-assigned-to-tree",
        description: "The user is not actively assigned to care for this tree.");

    public static Error CareTypeNotFound(int careTypeId) => Error.NotFound(
        code: "care-type/not-found",
        description: $"Care type with ID {careTypeId} was not found.");
}
