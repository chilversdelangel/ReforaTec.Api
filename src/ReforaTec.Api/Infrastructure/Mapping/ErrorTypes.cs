namespace ReforaTec.Api.Infrastructure.Mapping;

internal static class ErrorTypes
{
    public const string Prefix = "https://api.reforatec.com/errors/";

    public const string Validation = $"{Prefix}validation";
    public const string BadRequest = $"{Prefix}bad-request";
    public const string InternalServer = $"{Prefix}internal-server";
}
