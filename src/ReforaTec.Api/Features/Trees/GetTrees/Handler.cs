using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.Trees.GetTrees;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role == UserRole.Student)
        {
            return Errors.Forbidden;
        }

        var trees = await GetTenantTreesAsync(context, loggedUser.TenantId, cancellationToken);
        if (trees.Count == 0)
        {
            return new Response([]);
        }

        var treeIds = trees.Select(tree => tree.Id).ToList();

        var latestHealthStates =
            await GetLatestHealthStatesAsync(context, loggedUser.TenantId, treeIds, cancellationToken);
        var assignedStudents =
            await GetActiveAssignedStudentsAsync(context, loggedUser.TenantId, treeIds, cancellationToken);

        var summaries = MapToSummaryDtos(trees, latestHealthStates, assignedStudents);
        return new Response(summaries);
    }

    private static Task<List<TreeProjection>> GetTenantTreesAsync(
        AppDbContext context,
        int tenantId,
        CancellationToken cancellationToken) =>
        context.Trees
            .AsNoTracking()
            .Where(tree => tree.TenantId == tenantId)
            .OrderBy(tree => tree.Folio)
            .ThenBy(tree => tree.Id)
            .Select(tree => new TreeProjection(
                Id: tree.Id,
                Folio: tree.Folio,
                SpeciesId: tree.SpeciesId,
                CommonName: tree.Species != null ? tree.Species.CommonName : string.Empty,
                ScientificName: tree.Species != null ? tree.Species.ScientificName : string.Empty,
                VirtueName: tree.Virtue != null ? tree.Virtue.Name : string.Empty,
                Location: tree.Location != null
                    ? new TreeLocationDto(tree.Location.Latitude, tree.Location.Longitude)
                    : null))
            .ToListAsync(cancellationToken);

    private static Task<Dictionary<int, TreeHealthState>> GetLatestHealthStatesAsync(
        AppDbContext context,
        int tenantId,
        IReadOnlyCollection<int> treeIds,
        CancellationToken cancellationToken) =>
        context.Inspections
            .AsNoTracking()
            .Where(inspection => inspection.TenantId == tenantId)
            .Where(inspection => treeIds.Contains(inspection.TreeId))
            .GroupBy(inspection => inspection.TreeId)
            .Select(treeGroup => treeGroup
                .OrderByDescending(inspection => inspection.DeviceCapturedAt)
                .ThenByDescending(inspection => inspection.Id)
                .Select(inspection => new
                {
                    inspection.TreeId,
                    inspection.HealthState
                })
                .First())
            .ToDictionaryAsync(
                inspection => inspection.TreeId,
                inspection => inspection.HealthState,
                cancellationToken);

    private static async Task<Dictionary<int, List<AssignedStudentDto>>> GetActiveAssignedStudentsAsync(
        AppDbContext context,
        int tenantId,
        IReadOnlyCollection<int> treeIds,
        CancellationToken cancellationToken)
    {
        var assignments = await context.CareAssignments
            .AsNoTracking()
            .Where(assignment => assignment.TenantId == tenantId)
            .Where(assignment => treeIds.Contains(assignment.TreeId))
            .Where(assignment => assignment.EndDate == null)
            .Where(assignment => assignment.User != null)
            .Select(assignment => new
            {
                assignment.TreeId,
                Student = new AssignedStudentDto(
                    assignment.User!.Id,
                    assignment.User.DisplayName,
                    assignment.User.ControlNumber)
            })
            .ToListAsync(cancellationToken);

        return assignments
            .GroupBy(
                item => item.TreeId,
                item => item.Student)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());
    }

    private static List<TreeSummaryDto> MapToSummaryDtos(
        IReadOnlyList<TreeProjection> trees,
        IReadOnlyDictionary<int, TreeHealthState> latestHealthStates,
        IReadOnlyDictionary<int, List<AssignedStudentDto>> assignedStudents) =>
        trees
            .Select(tree => new TreeSummaryDto(
                Id: tree.Id,
                Folio: tree.Folio,
                SpeciesId: tree.SpeciesId,

                CommonName: tree.CommonName,
                ScientificName: tree.ScientificName,
                VirtueName: tree.VirtueName,

                HealthState: latestHealthStates.GetValueOrDefault(tree.Id, TreeHealthState.Healthy).ToString(),
                Location: tree.Location,
                AssignedStudents: assignedStudents.GetValueOrDefault(tree.Id, [])))
            .ToList();

    private sealed record TreeProjection(
        int Id,
        string Folio,
        int SpeciesId,
        string CommonName,
        string ScientificName,
        string VirtueName,
        TreeLocationDto? Location);
}
