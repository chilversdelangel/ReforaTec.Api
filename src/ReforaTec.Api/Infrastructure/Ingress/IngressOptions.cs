namespace ReforaTec.Api.Infrastructure.Ingress;

internal sealed class IngressOptions
{
    public const string SectionName = "Ingress";

    public string? RealIpHeader { get; init; }
}
