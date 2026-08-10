namespace ReforaTec.Api.Features.Auth.RegisterUser;

public record Request(
    string Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? SecondLastName,
    string ControlNumber,
    string? PhoneNumber);
