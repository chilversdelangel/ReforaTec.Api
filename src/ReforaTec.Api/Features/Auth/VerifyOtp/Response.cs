namespace ReforaTec.Api.Features.Auth.VerifyOtp;

public record Response(
    string AccessToken, 
    string RefreshToken);
