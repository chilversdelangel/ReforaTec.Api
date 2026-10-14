using ErrorOr;

namespace ReforaTec.Api.Features.Trees.CreateTreeService;

internal static class Errors
{
    public static readonly Error TreeNotFound = Error.NotFound(
        code: "tree/not-found",
        description: "The specified tree was not found in this tenant.");

    public static readonly Error UserNotAssignedToTree = Error.Forbidden(
        code: "user/not-assigned-to-tree",
        description: "The user is not actively assigned to care for this tree.");

    public static readonly Error ServiceTypeNotFound = Error.NotFound(
        code: "service-type/not-found",
        description: "The specified service type was not found.");
}
