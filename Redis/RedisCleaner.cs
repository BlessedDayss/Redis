using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RedisCleaner
{
    public class RedisCacheCleaner
    { 
        public bool ClearRedisCache()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                
                return ExecuteCommand("wsl", "redis-cli FLUSHDB");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ExecuteCommand("redis-cli", "FLUSHDB");
            }else
            {
                throw new NotSupportedException("Unsupported OS platform");
            }
        }
        private bool ExecuteCommand(string command, string arguments)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo };

                process.Start();

                var output = process.StandardOutput.ReadToEnd();
                var errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"Redis return: {output}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error clearing Redis (ExitCode={process.ExitCode} + {errors}):");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occurred:");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
