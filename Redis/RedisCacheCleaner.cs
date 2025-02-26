namespace RedisCleaner
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class RedisCacheCleaner
    {
        public bool ClearRedisCache(int databaseNumber)
        {
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
                throw new PlatformNotSupportedException("This operation is only supported on Windows or Linux");

            string commandArguments = $"-n {databaseNumber} FLUSHDB";
            var result = RunProcess("redis-cli", commandArguments, 20000);

            if (result.timedOut)
            {
                Console.WriteLine("Redis is not responding (timeout). The server might be overloaded or unavailable.");
                return false;
            }

            // Если процесс завершился успешно, но есть ошибки в stderr
            if (result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.errors))
            {
                Console.WriteLine(
                    $"Redis executed with errors: '{result.errors.Trim()}', Output: '{result.output.Trim()}'");
                return false;
            }

            if (result.exitCode == 0)
            {
                if (result.output.Contains("OK"))
                {
                    Console.WriteLine($"Redis cleaned successfully. Output: '{result.output.Trim()}'");
                    return true;
                }
                else
                {
                    Console.WriteLine("Command executed:");
                    Console.WriteLine(result.output);
                    return true;
                }
            }
            else
            {
                return HandleProcessErrors(result.errors);
            }
        }

        private (int exitCode, string output, string errors, bool timedOut) RunProcess(string command, string arguments,
            int timeout)
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

            try
            {
                using var process = new Process { StartInfo = processInfo };
                process.Start();

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                bool exited = process.WaitForExit(timeout);
                if (!exited)
                {
                    process.Kill();
                    return (-1, string.Empty, string.Empty, true);
                }

                return (process.ExitCode, outputTask.Result, errorTask.Result, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception executing command '{command} {arguments}': {ex.Message}");
                return (-1, string.Empty, ex.Message, false);
            }
        }

        private bool HandleProcessErrors(string errors)
        {
            if (errors.Contains("Connection refused"))
            {
                Console.WriteLine("No connection to Redis. Server is stopped or unavailable.");
            }
            else if (errors.Contains("WRONGPASS") || errors.Contains("NOAUTH"))
            {
                Console.WriteLine("Redis authentication error. Check your password.");
            }
            else if (errors.Contains("No such file or directory"))
            {
                Console.WriteLine("Redis database not found at the specified path.");
            }
            else if (errors.Contains("Permission denied"))
            {
                Console.WriteLine("Access denied. Check Redis access permissions.");
            }
            else
            {
                Console.WriteLine($"Redis error: {errors}");
            }

            return false;
        }
    }
}