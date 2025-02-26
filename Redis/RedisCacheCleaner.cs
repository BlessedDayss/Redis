namespace RedisCleaner;

using System.Diagnostics;

public class RedisCacheCleaner
{
    private const int DefaultTimeout = 20000;

    public bool ClearRedisCache(int databaseNumber) {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("This operation is only supported on Windows or Linux");
        var commandArguments = $"-n {databaseNumber} FLUSHDB";
        (int exitCode, string output, string errors, bool timedOut) result = RunProcess("redis-cli", commandArguments,
            DefaultTimeout);
        if (result.timedOut) {
            LogError("Redis is not responding (timeout). The server might be overloaded or unavailable.");
            return false;
        }
        if (result.exitCode != 0)
            return HandleProcessErrors(result.errors);
        if (!string.IsNullOrWhiteSpace(result.errors)) {
            LogError($"Redis executed with errors: '{result.errors.Trim()}', Output: '{result.output.Trim()}'");
            return false;
        }
        if (result.output.Contains("OK")) {
            LogSuccess($"Redis cleaned successfully. Output: '{result.output.Trim()}'");
        } else {
            LogInfo("Command executed:");
            LogInfo(result.output);
        }
        return true;
    }

    private (int exitCode, string output, string errors, bool timedOut) RunProcess(string command, string arguments,
        int timeout) {
        var processInfo = new ProcessStartInfo {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        try {
            using var process = new Process {
                StartInfo = processInfo
            };
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            bool exited = process.WaitForExit(timeout);
            if (!exited) {
                process.Kill();
                return (-1, string.Empty, string.Empty, true);
            }
            return (process.ExitCode, outputTask.Result, errorTask.Result, false);
        } catch (Exception ex) {
            LogError($"Exception executing command '{command} {arguments}': {ex.Message}");
            return (-1, string.Empty, ex.Message, false);
        }
    }

    private bool HandleProcessErrors(string errors) {
        if (string.IsNullOrWhiteSpace(errors)) {
            LogError("Unknown Redis error occurred.");
            return false;
        }
        if (errors.Contains("Connection refused"))
            LogError("No connection to Redis. Server is stopped or unavailable.");
        else if (errors.Contains("WRONGPASS") || errors.Contains("NOAUTH"))
            LogError("Redis authentication error. Check your password.");
        else if (errors.Contains("No such file or directory"))
            LogError("Redis database not found at the specified path.");
        else if (errors.Contains("Permission denied"))
            LogError("Access denied. Check Redis access permissions.");
        else
            LogError($"Redis error: {errors}");
        return false;
    }
    
    private void LogError(string message) {
        Console.WriteLine(message);
    }
    
    private void LogSuccess(string message) {
        Console.WriteLine(message);
    }
    
    private void LogInfo(string message) {
        Console.WriteLine(message);
    }
}
