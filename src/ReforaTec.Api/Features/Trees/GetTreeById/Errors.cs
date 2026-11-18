using ErrorOr;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal static class Errors
{
    public static Error TreeNotFound(int treeId) => Error.NotFound(
        code: "tree/not-found",
        description: $"Tree with ID {treeId} was not found.");
}
