namespace ReforaTec.Api.Features.Auth.RefreshSession;

public record Request(string RefreshToken, string Audience);
