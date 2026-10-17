using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

internal sealed record Request(
    string Email,
    string GivenName,
    string FamilyName,
    string? SecondFamilyName,
    string ControlNumber,
    string? PhoneNumber,
    Gender Gender);
