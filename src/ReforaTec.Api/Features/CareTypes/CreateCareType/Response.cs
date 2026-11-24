namespace ReforaTec.Api.Features.CareTypes.CreateCareType;

internal sealed record Response(
    int Id,
    string Name,
    string IconUrl,
    bool IsActive);
