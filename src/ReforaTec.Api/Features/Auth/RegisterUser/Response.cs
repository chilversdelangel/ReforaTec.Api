using ReforaTec.Api.Entities.Enums;

namespace ReforaTec.Api.Features.Auth.RegisterUser;

internal sealed record Response(
    int Id,
    int TenantId,
    string Email,
    string FirstName,
    string LastName,
    string? SecondLastName,
    string? DisplayName,
    string? ControlNumber,
    string? PhoneNumber,
    Gender Gender,
    UserRole CurrentRole,
    DateTime CreatedAt);
