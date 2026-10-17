using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Users.UpdateUserProfile;

internal sealed record Request(
    string? FirstName = null,
    string? LastName = null,
    string? SecondLastName = null,
    string? DisplayName = null,
    string? PhoneNumber = null,
    Gender? Gender = null);
