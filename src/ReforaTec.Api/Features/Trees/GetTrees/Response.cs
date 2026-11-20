namespace ReforaTec.Api.Features.Trees.GetTrees;

internal sealed record Response(IReadOnlyList<TreeSummaryDto> Trees);

internal sealed record TreeSummaryDto(
    int Id,
    string Folio,
    int SpeciesId,
    string CommonName,
    string ScientificName,
    string VirtueName,
    string HealthState,
    TreeLocationDto? Location,
    IReadOnlyList<AssignedStudentDto> AssignedStudents);

internal sealed record TreeLocationDto(
    double? Latitude,
    double? Longitude);

internal sealed record AssignedStudentDto(
    int Id,
    string? DisplayName,
    string? ControlNumber);
