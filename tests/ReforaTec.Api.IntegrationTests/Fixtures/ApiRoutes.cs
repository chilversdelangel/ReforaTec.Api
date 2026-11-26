namespace ReforaTec.Api.IntegrationTests.Fixtures;

public static class ApiRoutes
{
    public const string Health = "/health";

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
        public const string CampaignEnrollment = $"{Me}/campaign-enrollment";
    }

    public static class CampaignEnrollments
    {
        public const string BaseRoute = $"{Base}/campaign-enrollments";
        public static string ById(int enrollmentId) => $"{BaseRoute}/{enrollmentId}";
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
        public static string Enrollments(int campaignId) => $"{BaseRoute}/{campaignId}/enrollments";
        public static string Close(int campaignId) => $"{BaseRoute}/{campaignId}/close";
    }

    public static class CareAssignments
    {
        private const string BaseRoute = $"{Base}/care-assignments";
        public static string ById(int assignmentId) => $"{BaseRoute}/{assignmentId}";
        public static string ByCampaign(int campaignId) => $"{Campaigns.BaseRoute}/{campaignId}/care-assignments";
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
