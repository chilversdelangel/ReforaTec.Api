using ErrorOr;

namespace ReforaTec.Api.Features.Species.CreateSpecies;

internal static class Errors
{
    public static Error DuplicateScientificName(string scientificName) => Error.Conflict(
        code: "species/duplicate-scientific-name",
        description: $"A species with scientific name '{scientificName}' already exists.");
}
