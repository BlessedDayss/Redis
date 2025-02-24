using System;
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
                return ExecuteCommand("redis-cli", "FLUSHDB");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ExecuteCommand("redis-cli", "FLUSHDB");
            }
            else
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
                    Console.WriteLine($"Redis return: {output.Trim()}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error clearing Redis (ExitCode={process.ExitCode}): {errors.Trim()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception executing command '{command} {arguments}': {ex.Message}");
                return false;
            }
        }
    }
}