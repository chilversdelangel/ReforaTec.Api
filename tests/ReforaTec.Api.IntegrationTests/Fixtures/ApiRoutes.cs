namespace ReforaTec.Api.IntegrationTests.Fixtures;

public static class ApiRoutes
{
    private const string Base = "/v1";

    public static class Auth
    {
        public const string OtpCodes = $"{Base}/otp-codes";
        public const string Sessions = $"{Base}/sessions";
        public const string RefreshSession = $"{Sessions}/refresh";
        public const string RevokeSession = $"{Sessions}/revoke";
        public const string Register = $"{Base}/users";
    }

    public static class Users
    {
        public const string BaseRoute = $"{Base}/users";
        public static string ById(int userId) => $"{BaseRoute}/{userId}";
        public const string Me = $"{BaseRoute}/me";
        public const string Profile = $"{Me}/profile";
        public const string MyTrees = $"{Me}/trees";
    }

    public static class CampaignEnrollments
    {
        public const string BaseRoute = $"{Base}/campaign-enrollments";
    }

    public static class Trees
    {
        public const string BaseRoute = $"{Base}/trees";
        public static string ById(int id) => $"{BaseRoute}/{id}";
        public static string CareLogs(int treeId) => $"{BaseRoute}/{treeId}/care-logs";
        public static string Inspections(int treeId) => $"{BaseRoute}/{treeId}/inspections";
        public static string Measurements(int treeId) => $"{BaseRoute}/{treeId}/measurements";
    }

    public static class Campaigns
    {
        public const string BaseRoute = $"{Base}/campaigns";
        public static string ById(int id) => $"{BaseRoute}/{id}";
        public static string CareAssignments(int campaignId) => $"{BaseRoute}/{campaignId}/care-assignments";
        public static string Enrollments(int campaignId) => $"{BaseRoute}/{campaignId}/enrollments";
    }

    public static class CareTypes
    {
        public const string BaseRoute = $"{Base}/care-types";
    }

    public static class Species
    {
        public const string BaseRoute = $"{Base}/species";
    }

    public static class Virtues
    {
        public const string BaseRoute = $"{Base}/virtues";
    }
}
