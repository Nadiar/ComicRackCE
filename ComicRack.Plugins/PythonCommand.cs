using System;
using System.IO;
using Python.Runtime;
using ComicRack.Plugins;
using cYo.Projects.ComicRack.Engine;

namespace cYo.Projects.ComicRack.Plugins
{
    // Minimal stub for PythonCommand after migration to pythonnet.
    public class PythonCommand : Command
    {
        // Path to the script file (relative or absolute).
        public string ScriptFile { get; set; }
        // Name of the method/function to invoke within the script.
        public string Method { get; set; }
        // Optional library path (not used in stub).
        public string LibPath { get; private set; }

        // Constructor – can set defaults if needed.
        // Stub for CompileExpression used by expression matchers.
        // Static stubs for compatibility with Program.cs
        public static bool Optimized { get; set; }
        public static Stream Output { get; set; }
        public static bool EnableLog { get; set; }

        public static T CompileExpression<T>(string expression, string[] variables) where T : Delegate
        {
            // For the specific Func<ComicBook, IComicBookStatsProvider, bool> used in the codebase,
            // return a simple lambda that always returns false. This satisfies compilation and
            // provides a safe fallback during migration.
            if (typeof(T) == typeof(Func<ComicBook, IComicBookStatsProvider, bool>))
            {
                var func = new Func<ComicBook, IComicBookStatsProvider, bool>((book, stats) => false);
                return (T)(object)func;
            }
            // For any other delegate types, throw to indicate unimplemented.
            throw new NotImplementedException($"CompileExpression not implemented for delegate type {typeof(T)}");
        }

        // The core execution method. It loads the script via PythonRuntimeManager
        // and, if a method name is provided, attempts to invoke it.
        protected override object OnInvoke(object[] data)
        {
            var manager = PythonRuntimeManager.Instance;
            manager.Initialize();

            // Inject the environment (API) so that 'ComicRack' is available in the script scope
            if (Environment != null)
            {
                manager.SetApi(Environment);
            }

            // Resolve full script path relative to the application base directory.
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScriptFile ?? string.Empty);
            
            // Add script folder to search path dynamically
            string scriptDir = Path.GetDirectoryName(scriptPath);
            manager.AddSearchPath(scriptDir);
            
            manager.LoadScript(scriptPath);

            // If a method name is specified, call it and return the result.
            if (!string.IsNullOrEmpty(Method))
            {
                try
                {
                    return manager.CallFunction(Method, data);
                }
                catch (Exception ex)
                {
                    // Log the error as Error so it appears red in the console
                    LogManager.Error("Script", $"Error invoking Python method '{Method}': {ex.Message}");
                    throw;
                }
            }

            // No method to invoke – return null.
            return null;
        }

        protected override void Log(string text, params object[] o)
        {
            // Use the centralized LogManager instead of writing to a stream manually
            if (EnableLog)
            {
                string message = (o != null && o.Length > 0) ? string.Format(text, o) : text;
                LogManager.Info("Script", message);
            }
        }
    }
}
