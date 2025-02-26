namespace RedisCleaner;

using System.Diagnostics;

public class RedisExecutor
{
    public static bool ClearRedisCache(string redisDB) {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("This operation is only supported on Windows or Linux");
        var arguments = $"-n {redisDB} FLUSHDB";
        return ExecuteCommand(arguments);
    }

    private static bool ExecuteCommand(string arguments) {
        var command = "redis-cli";
        try {
            var processInfo = new ProcessStartInfo {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = new Process {
                StartInfo = processInfo
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode == 0) {
                // Успешное выполнение, но проверяем наличие ошибок
                if (!string.IsNullOrWhiteSpace(errors)) {
                    Console.WriteLine($"Redis executed with errors: {errors.Trim()}, Output: '{output.Trim()}'");
                    return false;
                }

                // Вывод результата
                if (output.Contains("OK")) {
                    Console.WriteLine($"Redis cleaned successfully. Output: '{output.Trim()}'");
                } else {
                    Console.WriteLine("Command executed:");
                    Console.WriteLine(output);
                }
                return true;
            }
            return HandleProcessErrors(errors);
        } catch (Exception ex) {
            Console.WriteLine(
                $"Exception executing command '{command} {arguments}': {ex.Message}, Please run this command manualy");
            return false;
        }
    }

    private static bool HandleProcessErrors(string errors) {
        if (string.IsNullOrWhiteSpace(errors)) {
            Console.WriteLine("Unknown Redis error occurred.");
            return false;
        }
        if (errors.Contains("Connection refused"))
            Console.WriteLine("No connection to Redis. Server is stopped or unavailable.");
        else if (errors.Contains("WRONGPASS") || errors.Contains("NOAUTH"))
            Console.WriteLine("Redis authentication error. Check your password.");
        else if (errors.Contains("No such file or directory"))
            Console.WriteLine("Redis database not found at the specified path.");
        else if (errors.Contains("Permission denied"))
            Console.WriteLine("Access denied. Check Redis access permissions.");
        else
            Console.WriteLine($"Redis error: {errors}");
        return false;
    }
}
