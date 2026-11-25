namespace ReforaTec.Api.Features.CampaignEnrollments.EvaluateCampaignEnrollment;

internal sealed record Request(
    bool IsApproved,
    string? EvaluationNotes = null);
