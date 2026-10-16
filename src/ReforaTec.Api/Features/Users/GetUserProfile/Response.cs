using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Users.GetUserProfile;

internal sealed record Response(
    string FirstName,
    string LastName,
    string? SecondLastName,
    string? DisplayName,
    string? ControlNumber,
    string? PhoneNumber,
    Gender Gender,
    string Email,
    string? CurrentCampaign,
    int TotalTrees,
    int TotalServices);
