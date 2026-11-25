using ErrorOr;

namespace ReforaTec.Api.Features.CareAssignments.CloseCareAssignment;

internal static class Errors
{
    public static readonly Error Forbidden = Error.Forbidden(
        code: "auth/forbidden",
        description: "You do not have permission to close care assignments.");

    public static Error AssignmentNotFound(int assignmentId) => Error.NotFound(
        code: "care-assignment/not-found",
        description: $"Care assignment with ID {assignmentId} was not found.");

    public static readonly Error AlreadyClosed = Error.Conflict(
        code: "care-assignment/already-closed",
        description: "This care assignment has already been closed.");

    public static readonly Error EndDateBeforeStartDate = Error.Validation(
        code: "care-assignment/invalid-end-date",
        description: "End date cannot be earlier than the assignment start date.");
}
