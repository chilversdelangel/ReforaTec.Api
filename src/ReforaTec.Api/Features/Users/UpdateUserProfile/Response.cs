using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed record Response(
    int Id,
    string FirstName,
    string LastName,
    string? SecondLastName,
    string? DisplayName,
    string? ControlNumber,
    string? PhoneNumber,
    Gender Gender,
    string Email,
    UserRole CurrentRole);
