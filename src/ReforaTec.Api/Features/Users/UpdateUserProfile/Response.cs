namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed record Response(
    int Id,
    string GivenName,
    string FamilyName,
    string? SecondFamilyName,
    string? DisplayName,
    string? ControlNumber,
    string? PhoneNumber,
    string Gender,
    string Email,
    string Role);
