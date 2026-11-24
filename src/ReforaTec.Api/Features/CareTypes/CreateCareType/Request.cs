namespace ReforaTec.Api.Features.CareTypes.CreateCareType;

internal sealed record Request(
    string Name,
    string IconUrl,
    bool IsActive = true);
