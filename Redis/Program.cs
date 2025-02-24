using Microsoft.Extensions.Configuration;

namespace RedisCleaner
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build();
            
            bool clearRedisCache = configuration.GetValue<bool>("Feature:ClearRedisCache", true);

            Console.WriteLine($"Clearing Redis cache: {clearRedisCache}");

            if (clearRedisCache)
            {
                var cleanerCache = new RedisCleaner.RedisCacheCleaner();
                cleanerCache.ClearRedisCache();
            }
            else
            {
                Console.WriteLine("Skipping Redis cache cleaning.");
            }
            
            // Added for testing purposes
            // Uncomment to test the RedisCacheCleaner class
            // 
            //
            // var cleaner = new RedisCacheCleaner();
            //
            // if (cleaner.ClearRedisCache())
            // {
            //     Console.WriteLine("Cache cleaned!");
            // }
            // else
            // {
            //     Console.WriteLine("Cache was not cleaned due to an error.");
            // }
        }
    }
}