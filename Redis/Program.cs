namespace RedisCleaner
{
    internal static class Program
    {
        static void Main()
        {
            var cleaner = new RedisCacheCleaner();

            if (cleaner.ClearRedisCache())
            {
                Console.WriteLine("Cache cleaned!");
            }
            else
            {
                Console.WriteLine("Cache was not cleaned due to an error.");
            }
        }
    }
}