namespace RedisCleaner
{
    using Microsoft.Extensions.Configuration;

    internal static class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build();
            
            bool skipClearRedisCache = configuration.GetValue<bool>("Features:SkipClearRedisCache");
            string redisDatabase = configuration.GetValue<string>("Redis:Database") ?? "0";
            
            Console.WriteLine($"Clearing Redis cache: {!skipClearRedisCache}");
        
            if (!skipClearRedisCache)
            {
                RedisExecutor.ClearRedisCache(redisDatabase);
            }
            else
            {
                Console.WriteLine("Skipping Redis cache cleaning.");
            }
        }
    }
}
