namespace Middleware.Services.Cache
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? ttl = null);
        void Remove(string key);
    }
}