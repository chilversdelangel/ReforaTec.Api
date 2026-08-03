namespace ReforaTec.Api.Features.Auth.RotateToken;

public record Request(string RefreshToken, string Audience);
