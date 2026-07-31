namespace ReforaTec.Api.Infrastructure.Security.Jwt;

/// <summary>
/// Valid JWT audience identifiers for ReforaTec client applications.
/// Values must match the ValidAudiences list in appsettings.json.
/// </summary>
public static class Audience
{
    public const string MobileApp = "ReforaTec.MobileApp";
    public const string AdminDashboard = "ReforaTec.AdminDashboard";
}
