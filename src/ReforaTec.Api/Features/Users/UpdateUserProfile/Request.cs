using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed record Request(
    string? GivenName = null,
    string? FamilyName = null,
    string? SecondFamilyName = null,
    string? DisplayName = null,
    string? PhoneNumber = null,
    Gender? Gender = null);
