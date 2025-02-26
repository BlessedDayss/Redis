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


            bool clearRedisCache = configuration.GetValue<bool>("Features:SkipClearRedisCache");
            Console.WriteLine($"Clearing Redis cache: {clearRedisCache}");
        
            if (clearRedisCache)
            {
                var cleaner = new RedisCacheCleaner();
                bool result = cleaner.ClearRedisCache(databaseNumber: 15);
            }
            else
            {
                Console.WriteLine("Skipping Redis cache cleaning.");
            }
        }
    }
}