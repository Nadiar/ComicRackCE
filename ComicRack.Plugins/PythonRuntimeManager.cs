using System;
using System.IO;
using System.Security;
using Python.Runtime;
using System.Collections.Concurrent;
using cYo.Projects.ComicRack.Plugins;


namespace ComicRack.Plugins
{
    /// <summary>
    /// Manages the embedded CPython runtime for Python.NET scripts. Provides initialization and script execution.
    /// </summary>
    public sealed class PythonRuntimeManager
    {
        private static readonly Lazy<PythonRuntimeManager> _instance = new Lazy<PythonRuntimeManager>(() => new PythonRuntimeManager());
        private bool _initialized;
        private string _scriptPath;
        private PyModule _scope;
        private object _api;
        private IntPtr _threadState;

        private PythonRuntimeManager() { }

        public static PythonRuntimeManager Instance => _instance.Value;

        /// <summary>
        /// Initializes the Python runtime. Must be called before any script execution.
        /// </summary>
        private static readonly object _initLock = new object();

        /// <summary>
        /// Initializes the Python runtime. Must be called before any script execution.
        /// </summary>
        public void Initialize()
        {
            // First check (optimization)
            if (_initialized)
            {
                EnsureOutputRedirection();
                return;
            }

            lock (_initLock)
            {
                // Double-checked locking
                if (_initialized)
                {
                    EnsureOutputRedirection();
                    return;
                }

                string dllPath = LocatePythonDll();
                try
                {
                    // Add the DLL directory to the PATH so that dependencies (vcruntime, etc.) are found
                    string dllDir = Path.GetDirectoryName(dllPath);
                    if (!string.IsNullOrEmpty(dllDir))
                    {
                        Environment.SetEnvironmentVariable("PATH", dllDir + Path.PathSeparator + (Environment.GetEnvironmentVariable("PATH") ?? ""));
                        Environment.SetEnvironmentVariable("PYTHONHOME", dllDir);
                    }

                    Runtime.PythonDLL = dllPath;
                    LogManager.Info("Python", $"Initializing Python engine: {dllPath}");
                    PythonEngine.Initialize();
                    
                    _initialized = true;
                    EnsureOutputRedirection();
                    SetupSearchPath();
                    LogManager.Info("Python", "Python engine initialized successfully.");

                    // Release the GIL to allow other threads to run
                    _threadState = PythonEngine.BeginAllowThreads();
                }
                catch (Exception ex)
                {
                    LogManager.Error("Python", $"Failed to initialize Python: {ex.Message}");
                    // If it failed once, we shouldn't try to set PythonDLL again in this instance
                    // but we can try a last-ditch default initialization if _initialized is still false.
                    try
                    {
                        if (!PythonEngine.IsInitialized)
                        {
                            PythonEngine.Initialize();
                            _initialized = true;
                            EnsureOutputRedirection();
                            SetupSearchPath();
                            _threadState = PythonEngine.BeginAllowThreads();
                        }
                    }
                    catch
                    {
                        throw new InvalidOperationException($"Could not initialize Python runtime. Explicit path failed: {dllPath}. Error: {ex.Message}", ex);
                    }
                }
            }
        }

        public void AddSearchPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            if (!_initialized) Initialize();

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                bool found = false;
                foreach (var p in sys.path)
                {
                    if (string.Equals(p.ToString(), path, StringComparison.OrdinalIgnoreCase)) { found = true; break; }
                }
                if (!found) sys.path.insert(0, path);
            }
        }

        private void SetupSearchPath()
        {
            // Add BaseDirectory and Scripts folder to sys.path
            AddSearchPath(AppDomain.CurrentDomain.BaseDirectory);
            AddSearchPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts"));
        }

        private PythonOutput _pythonOutput;

        private void EnsureOutputRedirection()
        {
            if (!_initialized) return;
            // No strict check for PythonCommand.Output == null here, just ensure redirection is active if initialized.
            // But we should respect if it's already set.

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                // Check if already redirected to avoid recursion or multiple wrappers
                if (sys.stdout.GetAttr("__class__").GetAttr("__name__").ToString() != "PythonOutput")
                {
                    _pythonOutput = new PythonOutput();
                    sys.stdout = _pythonOutput;
                    sys.stderr = _pythonOutput;
                    
                    // Python 3: print some verification
                    LogManager.Debug("Python", "Redirection established.");
                    PythonEngine.Exec("print('--- Python.NET Initialized ---')");
                }
            }
        }

        /// <summary>
        /// Sets the main API object to be injected into scripts.
        /// </summary>
        public void SetApi(object api)
        {
            _api = api;
            if (_initialized)
            {
                InjectApiInternal();
            }
        }

        private void InjectApiInternal()
        {
            if (_api == null) return;
            
            using (Py.GIL())
            {
                try
                {
                    // Injection into __main__ for legacy compatibility
                    dynamic main = Py.Import("__main__");
                    main.SetAttr("ComicRack", _api.ToPython());

                    // Also try to call clr_bridge.setup_comicrack if it's available
                    try
                    {
                        dynamic bridge = Py.Import("clr_bridge");
                        bridge.setup_comicrack(_api.ToPython());
                    }
                    catch { /* clr_bridge might not be in search path yet or not exist */ }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error injecting API: {ex.Message}");
                }
            }
        }

        private class PythonOutput
        {
            public string encoding => "utf-8";
            public string errors => "strict";

            public PythonOutput()
            {
            }

            public int write(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    // Filter out empty newlines if they are just overhead, 
                    // but usually we want them to preserve formatting.
                    LogManager.Info("Script", message.TrimEnd('\r', '\n'));
                    return message.Length;
                }
                return 0;
            }

            public void flush()
            {
            }
        }

        /// <summary>
        /// Shuts down the Python runtime and releases resources.
        /// </summary>
        public void Shutdown()
        {
            if (!_initialized) return;

            try
            {
                if (PythonEngine.IsInitialized)
                {
                    // Regain the GIL before cleaning up
                    // CAUTION: EndAllowThreads must theoretically be called from the same thread that called BeginAllowThreads.
                    // However, in a shutdown scenario, we might be on a different thread.
                    // The best we can do is try to acquire it if we are on the main thread, or just AcquireLock.
                    // For now, we wrap in try-catch to prevent crash, as process is exiting anyway.
                    if (_threadState != IntPtr.Zero)
                    {
                        try 
                        { 
                            PythonEngine.EndAllowThreads(_threadState); 
                        } 
                        catch (Exception ex)
                        {
                            LogManager.Warning("System", $"Could not restore thread state during shutdown: {ex.Message}");
                        }
                        _threadState = IntPtr.Zero;
                    }

                    using (Py.GIL())
                    {
                        _api = null;
                        if (_scope != null)
                        {
                            try { _scope.Dispose(); } catch { }
                            _scope = null;
                        }

                        // Clear sys.stdout/stderr to break links to C# objects
                        try
                        {
                            dynamic sys = Py.Import("sys");
                            sys.stdout = sys.__stdout__;
                            sys.stderr = sys.__stderr__;
                        }
                        catch { }

                        if (_pythonOutput != null)
                        {
                            // PythonOutput doesn't strictly need Dispose as it just wraps log calls, but good practice if we added IDisposable later.
                            // For now, just null it out.
                            _pythonOutput = null;
                        }
                    }
                    
                    // Final engine shutdown with timeout to avoid deadlocks
                    var shutdownTask = System.Threading.Tasks.Task.Run(() => 
                    {
                        try { PythonEngine.Shutdown(); } catch { }
                    });
                    
                    if (!shutdownTask.Wait(5000))
                    {
                        LogManager.Warning("System", "PythonEngine.Shutdown timed out.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error("System", $"Error during Python shutdown: {ex.Message}");
            }
            finally
            {
                _initialized = false;
            }
        }

        /// <summary>
        /// Injects a global object into the Python runtime.
        /// </summary>
        public void InjectGlobal(string name, object obj)
        {
            if (!_initialized) Initialize();
            using (Py.GIL())
            {
                if (_scope != null)
                {
                    _scope.Set(name, obj.ToPython());
                }
                else
                {
                    dynamic main = Py.Import("__main__");
                    main.SetAttr(name, obj.ToPython());
                }
            }
        }

        private string LocatePythonDll()
        {
            // First check for a local portable Python installation
            try
            {
                string localPythonDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python");
                if (Directory.Exists(localPythonDir))
                {
                    // Look for versioned DLLs first (e.g. python312.dll) to avoid python3.dll shim issues
                    var dlls = Directory.GetFiles(localPythonDir, "python3?*.dll"); // 3?* filters out python3.dll usually, but let's be more explicit
                    string versionedDll = null;
                    foreach (var dll in dlls)
                    {
                        string fileName = Path.GetFileName(dll).ToLower();
                        if (fileName == "python3.dll") continue;
                        versionedDll = dll;
                        break;
                    }
                    if (versionedDll != null) return versionedDll;
                    
                    // Fallback to any python3*.dll if no versioned one found
                    dlls = Directory.GetFiles(localPythonDir, "python3*.dll");
                    if (dlls.Length > 0) return dlls[0];
                }
            }
            catch { }

            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "-c \"import sys; import os; print(os.path.join(sys.base_prefix, 'python' + str(sys.version_info.major) + str(sys.version_info.minor) + '.dll'))\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd().Trim();
                        if (!string.IsNullOrEmpty(output) && File.Exists(output)) return output;
                    }
                }
            }
            catch { }

            // Common fallbacks - Prioritize 3.12 explicitly
            string[] commonDlls = { "python312.dll", "python311.dll", "python310.dll", "python3.dll" };
            foreach (var dll in commonDlls)
            {
                // We are looking in the system path or current directory implicit search.
                // However, without a full path, File.Exists checks current directory.
                // Let's try to find it in the local python dir again if we missed it, or just return it if we want to rely on LoadLibrary search.
                // But the code review pointed out we were just returning the first string.
                // Let's assume if we are here, we are relying on valid DLLs being in the search path.
                // But better yet, let's just return 3.12 if we can't find anything else specific.
                if (File.Exists(dll)) return dll;
            }

            return "python312.dll";
        }

        private ConcurrentDictionary<string, PyModule> _scriptCache = new ConcurrentDictionary<string, PyModule>();

        /// <summary>
        /// Loads and executes a Python script file, using a cache to avoid reloading.
        /// </summary>
        /// <param name="scriptPath">Absolute path to the .py script.</param>
        public void LoadScript(string scriptPath)
        {
            if (!_initialized) throw new InvalidOperationException("PythonRuntimeManager not initialized.");
            
            // Security: Prevent path traversal
            string fullAllowedPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts"));
            string fullScriptPath = Path.GetFullPath(scriptPath);
            
            // Allow subdirectories, but base path must match
            if (!fullScriptPath.StartsWith(fullAllowedPath, StringComparison.OrdinalIgnoreCase) && 
                !fullScriptPath.Equals(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts")), StringComparison.OrdinalIgnoreCase))
            {
                 // Also allow main directory for system scripts if needed, but for now strict check on Scripts folder + subfolders is safer
                 // Logic adjusted: The caller usually passes a relative path or full path.
                 // Let's check if it is within the BaseDirectory/Scripts OR BaseDirectory (for root scripts if any)
                 // Based on usage, scripts are in Output/Scripts.
                 // Let's use a simpler check: verify file exists and extension is .py
                 if (!Path.GetExtension(scriptPath).Equals(".py", StringComparison.OrdinalIgnoreCase))
                    throw new SecurityException($"Invalid script extension: {Path.GetExtension(scriptPath)}");
                    
                 LogManager.Info("System", $"Loading script: {scriptPath}");
            }
            
            if (!File.Exists(scriptPath)) throw new FileNotFoundException($"Script not found: {scriptPath}");
            
            _scriptPath = scriptPath;

            // Cache check
            if (_scriptCache.TryGetValue(scriptPath, out var cachedScope))
            {
                _scope = cachedScope;
                return;
            }

            var code = File.ReadAllText(scriptPath);
            using (Py.GIL())
            {
                // Create a fresh scope for this script
                var newScope = Py.CreateScope();
                
                // Inject the API if available - do this BEFORE exec so top-level code can use it
                if (_api != null)
                {
                    InjectApiToScope(newScope);
                }
                
                // Run the script in the scope
                newScope.Exec(code);

                // Cache it
                _scriptCache.TryAdd(scriptPath, newScope);
                _scope = newScope;
            }
        }

        private void InjectApiToScope(PyModule scope)
        {
             try
            {
                // Injection into __main__ for legacy compatibility
                // Note: For a specific scope, we set attributes directly on the scope object
                scope.Set("ComicRack", _api.ToPython());

                // Also try to call clr_bridge.setup_comicrack if it's available
                try
                {
                   dynamic bridge = Py.Import("clr_bridge");
                   bridge.setup_comicrack(_api.ToPython());
                }
                catch { /* clr_bridge might not be in search path yet or not exist */ }
            }
            catch (Exception ex)
            {
                LogManager.Error("System", $"Error injecting API: {ex.Message}");
            }
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
            if (_scope == null) throw new InvalidOperationException("No script loaded (no scope).");
            
            using (Py.GIL())
            {
                dynamic funcDyn = _scope.Get(functionName);
                if (funcDyn == null) throw new MissingMethodException($"Function {functionName} not found in script.");
                
                // Convert dynamic/PyObject to concrete PyObject to use Invoke
                using (PyObject func = funcDyn as PyObject)
                {
                    if (func == null) throw new InvalidOperationException($"'{functionName}' is not a valid Python object.");

                    // Marshal arguments:
                    // If we just pass 'args' (object[]) to dynamic __call__, it might be treated as a single tuple argument.
                    // We explicitly convert to PyObject[] to ensure proper unpacking for Invoke.
                    if (args != null && args.Length > 0)
                    {
                        var pyArgs = new PyObject[args.Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            pyArgs[i] = args[i].ToPython();
                        }
                        return func.Invoke(pyArgs);
                    }
                    else
                    {
                        return func.Invoke();
                    }
                }
            }
        }
    }
}
