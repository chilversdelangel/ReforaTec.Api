namespace ReforaTec.Api.Infrastructure.Security.Jwt;

/// <summary>
/// Valid JWT audience identifiers for ReforaTec client applications.
/// Values must match the ValidAudiences list in appsettings.json.
/// </summary>
public static class Audience
{
    public const string StudentMobileApp = "student-mobile-app";
    public const string InspectorMobileApp = "inspector-mobile-app";
    public const string WebDashboard = "web-dashboard";
}
