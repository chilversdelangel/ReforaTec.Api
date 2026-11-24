namespace ReforaTec.Api.Features.Campaigns.GetCampaignEnrollments;

internal sealed record UserSummary(
    int Id,
    string GivenName,
    string FamilyName,
    string? SecondFamilyName,
    string? DisplayName,
    string Email,
    string? ControlNumber);

internal sealed record Response(
    int Id,
    UserSummary User,
    bool? IsApproved,
    string? EvaluationNotes,
    DateTimeOffset EnrolledAt,
    DateTimeOffset? EvaluatedAt);
