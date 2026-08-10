namespace ReforaTec.Api.Features.Auth.RegisterUser;

public record Response(
    int Id,
    int TenantId,
    string Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? SecondLastName,
    string? ControlNumber,
    string? PhoneNumber,
    string Role,
    DateTime CreatedAt);
