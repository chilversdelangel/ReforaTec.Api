using ErrorOr;

namespace ReforaTec.Api.Features.Virtues.CreateVirtue;

internal static class Errors
{
    public static Error DuplicateName(string name) => Error.Conflict(
        code: "virtue/duplicate-name",
        description: $"A virtue with name '{name}' already exists.");
}
