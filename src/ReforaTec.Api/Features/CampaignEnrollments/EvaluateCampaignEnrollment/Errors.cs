using ErrorOr;

namespace ReforaTec.Api.Features.CampaignEnrollments.EvaluateCampaignEnrollment;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to evaluate campaign enrollments.");

    public static Error EnrollmentNotFound(int enrollmentId) => Error.NotFound(
        code: "campaign-enrollment/not-found",
        description: $"Campaign enrollment with ID {enrollmentId} was not found.");
}
