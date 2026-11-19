using ErrorOr;

namespace ReforaTec.Api.Features.Trees.CreateTreeMeasurement;

internal static class Errors
{
    public static Error TreeNotFound(int treeId) => Error.NotFound(
        code: "tree/not-found",
        description: $"Tree with ID {treeId} was not found.");

    public static readonly Error InsufficientPermissions = Error.Forbidden(
        code: "auth/forbidden",
        description: "Students are not authorized to record tree measurements.");
}
