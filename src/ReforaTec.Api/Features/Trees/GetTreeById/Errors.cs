using ErrorOr;

namespace ReforaTec.Api.Features.Trees.GetTreeById;

internal static class Errors
{
    public static readonly Error TreeNotFound = Error.NotFound(
        code: "tree/not-found",
        description: "The specified tree was not found in this tenant.");
}
