using System;
using System.IO;
using Python.Runtime;

namespace ComicRack.Plugins
{
    /// <summary>
    /// Manages the embedded CPython runtime for Python.NET scripts.
    /// Provides initialization, script loading, and function invocation.
    /// </summary>
    public sealed class PythonRuntimeManager
    {
        private static readonly Lazy<PythonRuntimeManager> _instance = new Lazy<PythonRuntimeManager>(() => new PythonRuntimeManager());
        private bool _initialized;
        private PyScope _scope;

        private PythonRuntimeManager()
        {
            // Private constructor for singleton pattern.
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PythonRuntimeManager Instance => _instance.Value;

        /// <summary>
        /// Initializes the Python runtime. Must be called before any script execution.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            // Ensure the correct Python DLL is loaded. Adjust path if needed.
            // Assuming python312.dll is located next to the executable or in PATH.
            Runtime.PythonDLL = "python312.dll";
            PythonEngine.Initialize();
            _scope = Py.CreateScope();
            _initialized = true;
        }

        /// <summary>
        /// Loads a Python script file into the current scope.
        /// </summary>
        /// <param name="scriptPath">Absolute path to the .py script.</param>
        public void LoadScript(string scriptPath)
        {
            if (!_initialized) throw new InvalidOperationException("PythonRuntimeManager not initialized.");
            if (!File.Exists(scriptPath)) throw new FileNotFoundException($"Script not found: {scriptPath}");

            var code = File.ReadAllText(scriptPath);
            _scope.Exec(code);
        }

        /// <summary>
        /// Calls a function defined in the loaded script.
        /// </summary>
        /// <param name="functionName">Name of the Python function to call.</param>
        /// <param name="args">Arguments to pass to the function.</param>
        /// <returns>Result as a dynamic object.</returns>
        public dynamic CallFunction(string functionName, params object[] args)
        {
            if (!_initialized) throw new InvalidOperationException("PythonRuntimeManager not initialized.");
            var pyFunc = _scope.Get(functionName);
            if (pyFunc == null) throw new MissingMethodException($"Function '{functionName}' not found in Python scope.");
            using (var pyArgs = new PyTuple(args.Length))
            {
                for (int i = 0; i < args.Length; i++)
                {
                    pyArgs[i] = args[i].ToPython();
                }
                return pyFunc.Invoke(pyArgs).AsManagedObject(typeof(object));
            }
        }
    }
}
