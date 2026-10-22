namespace ReforaTec.Api.IntegrationTests.Fixtures;

public static class ApiRoutes
{
    private const string Base = "/api/v1";

    public static class Auth
    {
        public const string OtpCodes = $"{Base}/otp-codes";
        public const string Sessions = $"{Base}/sessions";
        public const string RefreshSession = $"{Base}/sessions/refresh";
        public const string RevokeSession = $"{Base}/sessions/revoke";
        public const string Register = $"{Base}/users";
    }

    public static class Users
    {
        public const string Me = $"{Base}/users/me";
        public const string Profile = $"{Base}/users/me/profile";
        public const string MyTrees = $"{Base}/users/me/trees";
    }

    public static class Trees
    {
        public const string BaseRoute = $"{Base}/trees";
        public static string ById(int id) => $"{Base}/trees/{id}";
        public static string Services(int treeId) => $"{Base}/trees/{treeId}/services";
    }

    public static class Campaigns
    {
        public const string BaseRoute = $"{Base}/campaigns";
        public static string ById(int id) => $"{Base}/campaigns/{id}";
    }
}
