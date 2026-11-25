namespace ReforaTec.Api.Features.CampaignEnrollments.EvaluateCampaignEnrollment;

internal sealed record Response(
    int Id,
    int CampaignId,
    int UserId,
    bool IsApproved,
    string? EvaluationNotes,
    DateTimeOffset EvaluatedAt);
