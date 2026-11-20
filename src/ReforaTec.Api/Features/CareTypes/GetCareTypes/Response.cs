namespace ReforaTec.Api.Features.CareTypes.GetCareTypes;

internal sealed record Response(IReadOnlyList<CareTypeDto> CareTypes);

internal sealed record CareTypeDto(
    int Id,
    string Name,
    string IconUrl);
