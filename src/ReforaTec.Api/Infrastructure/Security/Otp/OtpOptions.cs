namespace ReforaTec.Api.Infrastructure.Security.Otp;

internal sealed class OtpOptions
{
    public const string SectionName = "Otp";

    public int ExpirationMinutes { get; set; } = 5;
    public int MaxFailedAttempts { get; set; } = 3;
}
