using ErrorOr;

namespace ReforaTec.Api.Features.Trees.GetTrees;

internal static class Errors
{
    public static Error Forbidden => Error.Forbidden(
        code: "auth/forbidden",
        description: "Students are not allowed to list all tenant trees.");
}
