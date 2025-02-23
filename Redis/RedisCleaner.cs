using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RedisCleaner
{
    public class RedisCacheCleaner
    { 
        public bool ClearRedisCache()
        {
            // Check current OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: using WSL
                return ExecuteCommand("wsl", "redis-cli FLUSHDB");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: direct command
                return ExecuteCommand("redis-cli", "FLUSHDB");
            }
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

                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"Redis return: {output}");
                    return true;
                }
                
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
