namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public sealed record Response(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresInSeconds);