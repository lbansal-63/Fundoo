namespace Middleware.Services.Cache
{
    public static class CacheKeys
    {
        public static string TokenVersion(int userId) => $"tv:{userId}";
        public static string Notes(int userId, string view) => $"notes:{userId}:{view}";
    }
}