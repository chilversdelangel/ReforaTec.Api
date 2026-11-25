using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReforaTec.Api.Common.Helpers;
using ReforaTec.Api.Database;
using ReforaTec.Api.Entities;
using ReforaTec.Api.Entities.Enums;
using ReforaTec.Api.Infrastructure.Security.CurrentUser;

namespace ReforaTec.Api.Features.CampaignEnrollments.EvaluateCampaignEnrollment;

internal static class Handler
{
    public static async Task<ErrorOr<Response>> Handle(
        UserIdentity loggedUser,
        int enrollmentId,
        Request request,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (loggedUser.Role is not (UserRole.Coordinator or UserRole.SystemAdmin))
        {
            return Errors.Forbidden;
        }

        var enrollment = await FindEnrollmentAsync(context, loggedUser.TenantId, enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return Errors.EnrollmentNotFound(enrollmentId);
        }

        enrollment.IsApproved = request.IsApproved;
        enrollment.EvaluationNotes = request.EvaluationNotes?.ToSanitizedOrNull();

        await context.SaveChangesAsync(cancellationToken);

        return new Response(
            enrollment.Id,
            enrollment.CampaignId,
            enrollment.UserId,
            enrollment.IsApproved.Value,
            enrollment.EvaluationNotes,
            enrollment.ModifiedAt);
    }

    private static Task<CampaignEnrollment?> FindEnrollmentAsync(
        AppDbContext context,
        int tenantId,
        int enrollmentId,
        CancellationToken cancellationToken)
    {
        return context.CampaignEnrollments
            .Where(enrollment => enrollment.TenantId == tenantId)
            .Where(enrollment => enrollment.Id == enrollmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
