namespace RedisCleaner
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class RedisCacheCleaner
    {
        public bool ClearRedisCache()
        {
            // Simplify OS platform logic since the command is the same for both Windows and Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ExecuteCommand("FLUSHDB");
            }
            else
            {
                throw new NotSupportedException("Unsupported OS platform");
            }
        }

        private bool ExecuteCommand(string arguments)
        {
            var command = "redis-cli";
            try
            {
                // Check if redis-cli is installed
                if (!IsRedisCliInstalled())
                {
                    Console.WriteLine($"redis-cli is not installed. Please install Redis and try again.");
                    return false;
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process
                {
                    StartInfo = processInfo
                };

                process.Start();

                // Read output asynchronously with timeout to avoid blocking when there's no response
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                // Wait for process completion with timeout
                bool exited = process.WaitForExit(5000); // 5 seconds timeout

                if (!exited)
                {
                    process.Kill();
                    Console.WriteLine(
                        "Redis is not responding (timeout). The server might be overloaded or unavailable.");
                    return false;
                }

                string output = outputTask.Result;
                string errors = errorTask.Result;

                if (process.ExitCode == 0)
                {
                    // Successful execution
                    if (output.Contains("PONG"))
                    {
                        Console.WriteLine("Redis is working correctly.");
                        return true;
                    }
                    else if (output.Contains("OK"))
                    {
                        Console.WriteLine("Command executed successfully:");
                        Console.WriteLine(output);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Command executed:");
                        Console.WriteLine(output);
                        return true;
                    }
                }
                else
                {
                    // Analysis of specific Redis errors
                    if (errors.Contains("Connection refused"))
                    {
                        Console.WriteLine("No connection to Redis. Server is stopped or unavailable.");
                        return false;
                    }
                    else if (errors.Contains("WRONGPASS") || errors.Contains("NOAUTH"))
                    {
                        Console.WriteLine("Redis authentication error. Check your password.");
                        return false;
                    }
                    else if (errors.Contains("No such file or directory"))
                    {
                        Console.WriteLine("Redis database not found at the specified path.");
                        return false;
                    }
                    else if (errors.Contains("Permission denied"))
                    {
                        Console.WriteLine("Access denied. Check Redis access permissions.");
                        return false;
                    }
                    else
                    {
                        Console.WriteLine($"Redis error: {errors}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The system cannot find the file specified"))
                {
                    Console.WriteLine(
                        "redis-cli not found. Make sure Redis is installed and its path is added to the PATH variable.");
                }
                else
                {
                    Console.WriteLine($"Exception executing command '{command} {arguments}': {ex.Message}");
                    Console.WriteLine("Please run this command manually for debugging.");
                }

                return false;
            }
        }

        // Helper method to check if redis-cli is installed
        private bool IsRedisCliInstalled()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "redis-cli",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process
                {
                    StartInfo = processInfo
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}