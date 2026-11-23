namespace ReforaTec.Api.Features.Virtues.GetVirtues;

internal sealed record Response(IReadOnlyList<VirtueDto> Virtues);

internal sealed record VirtueDto(
    int Id,
    string Name);
