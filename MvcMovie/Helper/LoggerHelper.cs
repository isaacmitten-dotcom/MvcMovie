using System;
using Microsoft.Extensions.Logging;


namespace MvcMovie.Helper

{
    public static class LoggerHelper
    {
        public static void Info(this ILogger logger, string? message, params object[] args) => logger.LogInformation("{Time}: {message}" + message, DateTimeOffset.UtcNow, args);
        public static void Warn(this ILogger logger, string? message, params object[] args) => logger.LogWarning("{Time}: {message}" + message, DateTimeOffset.UtcNow, args);
        public static void Error(this ILogger logger, Exception ex, string? message, params object[] args) => logger.LogError(ex, "{Time}: {message}" + message, DateTimeOffset.UtcNow, args);
        public static void Debug(this ILogger logger, string? message, params object[] args) => logger.LogDebug("{Time}: {message}" + message, DateTimeOffset.UtcNow, args);
    }
}
