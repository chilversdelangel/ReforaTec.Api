using ErrorOr;

namespace ReforaTec.Api.Features.Trees.CreateTree;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to register new trees.");

    public static Error FolioAlreadyExists(string folio) => Error.Conflict(
        code: "tree/folio-already-exists",
        description: $"A tree with folio '{folio}' already exists.");

    public static Error SpeciesNotFound(int speciesId) => Error.NotFound(
        code: "species/not-found",
        description: $"Species with ID {speciesId} was not found.");

    public static Error VirtueNotFound(int virtueId) => Error.NotFound(
        code: "virtue/not-found",
        description: $"Virtue with ID {virtueId} was not found.");
}
