using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace cYo.Projects.ComicRack.Plugins
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] [{Level}] [{Source}] {Message}";
    }

    public static class LogManager
    {
        private static readonly ConcurrentQueue<LogEntry> _logs = new ConcurrentQueue<LogEntry>();
        private const int MaxLogs = 5000;

        public static event Action<LogEntry> LogAdded;

        public static IEnumerable<LogEntry> GetLogs() => _logs;

        public static void Log(LogLevel level, string source, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message
            };

            _logs.Enqueue(entry);
            
            // Keep memory in check
            while (_logs.Count > MaxLogs)
            {
                _logs.TryDequeue(out _);
            }

            LogAdded?.Invoke(entry);
        }

        public static void Trace(string source, string message) => Log(LogLevel.Trace, source, message);
        public static void Debug(string source, string message) => Log(LogLevel.Debug, source, message);
        public static void Info(string source, string message) => Log(LogLevel.Info, source, message);
        public static void Warning(string source, string message) => Log(LogLevel.Warning, source, message);
        public static void Error(string source, string message) => Log(LogLevel.Error, source, message);
    }
}
