using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

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
        private const long MaxLogFileSize = 50 * 1024 * 1024; // 50 MB per file
        private static readonly object _fileLock = new object();
        private static string _logPath;

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

            // Write to file with rotation support
            WriteToFile(entry);
        }

        private static string LogPath
        {
            get
            {
                if (_logPath == null)
                {
                    _logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "cYo", "ComicRack Community Edition", "ComicRack.log");
                    try
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_logPath));
                    }
                    catch { }
                }
                return _logPath;
            }
        }

        private static void WriteToFile(LogEntry entry)
        {
            try
            {
                lock (_fileLock)
                {
                    string path = LogPath;
                    
                    // Check size and rotate if needed (check rarely to save perf? or just check file info)
                    var fi = new System.IO.FileInfo(path);
                    if (fi.Exists && fi.Length > MaxLogFileSize)
                    {
                        RotateLog(path);
                        // Re-fetch info after rotation (it might be gone or empty)
                    }

                    System.IO.File.AppendAllText(path, entry.ToString() + Environment.NewLine);
                }
            }
            catch { /* Best effort logging */ }
        }

        private static void RotateLog(string currentPath)
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(currentPath);
                string name = System.IO.Path.GetFileNameWithoutExtension(currentPath);
                string ext = System.IO.Path.GetExtension(currentPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                string newPath = System.IO.Path.Combine(dir, $"{name}_{timestamp}{ext}");

                if (System.IO.File.Exists(currentPath))
                {
                    System.IO.File.Move(currentPath, newPath);
                    
                    // Cleanup old logs (keep last 5)
                    CleanUpOldLogs(dir, name, ext);
                }
            }
            catch { }
        }

        private static void CleanUpOldLogs(string dir, string name, string ext)
        {
            try
            {
                var pattern = $"{name}_*{ext}";
                var files = System.IO.Directory.GetFiles(dir, pattern)
                                     .Select(f => new System.IO.FileInfo(f))
                                     .OrderByDescending(f => f.LastWriteTime)
                                     .ToList();

                if (files.Count > 5)
                {
                    foreach (var file in files.Skip(5))
                    {
                        file.Delete();
                    }
                }
            }
            catch { }
        }

        public static void Trace(string source, string message) => Log(LogLevel.Trace, source, message);
        public static void Debug(string source, string message) => Log(LogLevel.Debug, source, message);
        public static void Info(string source, string message) => Log(LogLevel.Info, source, message);
        public static void Warning(string source, string message) => Log(LogLevel.Warning, source, message);
        public static void Error(string source, string message) => Log(LogLevel.Error, source, message);
    }
}
