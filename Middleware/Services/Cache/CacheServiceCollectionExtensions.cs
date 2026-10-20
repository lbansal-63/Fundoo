using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Middleware.Services.Cache
{
    public static class CacheServiceCollectionExtensions
    {
        public static IServiceCollection AddAppCaching(this IServiceCollection services, IConfiguration config)
        {
            var connection = config["Redis:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connection))
            {
                // Redis configure nahi hai to in-memory cache
                services.AddDistributedMemoryCache();
            }
            else
            {
                var options = ConfigurationOptions.Parse(connection);
                options.AbortOnConnectFail = false; // Redis down ho tab bhi app start ho
                options.ConnectTimeout = 2000;
                options.SyncTimeout = 2000;

                services.AddStackExchangeRedisCache(o =>
                {
                    o.ConfigurationOptions = options;
                    o.InstanceName = "fundoo:";
                });
            }

            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }
    }
}