using ErrorOr;

namespace ReforaTec.Api.Features.CareTypes.CreateCareType;

internal static class Errors
{
    public static Error DuplicateName(string name) => Error.Conflict(
        code: "care-type/duplicate-name",
        description: $"A care type with name '{name}' already exists.");
}
