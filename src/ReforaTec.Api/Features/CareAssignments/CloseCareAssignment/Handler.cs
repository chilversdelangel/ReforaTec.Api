using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.CareAssignments.CloseCareAssignment;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        int assignmentId,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var assignment = await FindAssignmentAsync(context, loggedUser.TenantId, assignmentId, cancellationToken);
        if (assignment is null)
        {
            return Errors.AssignmentNotFound(assignmentId);
        }

        if (assignment.EndDate is not null)
        {
            return Errors.AlreadyClosed;
        }

        if (request.EndDate < assignment.StartDate)
        {
            return Errors.EndDateBeforeStartDate;
        }

        assignment.EndDate = request.EndDate;
        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            assignment.Id,
            assignment.CampaignId,
            assignment.UserId,
            assignment.TreeId,
            assignment.StartDate,
            assignment.EndDate.Value);
    }

    private static Task<CareAssignment?> FindAssignmentAsync(
        AppDbContext context,
        int tenantId,
        int assignmentId,
        CancellationToken cancellationToken)
    {
        return context.CareAssignments
            .Where(assignment => assignment.TenantId == tenantId)
            .Where(assignment => assignment.Id == assignmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
