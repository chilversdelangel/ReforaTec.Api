namespace ReforaTec.Api.Infrastructure.Security.Cors;

internal sealed class CorsPolicyOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
