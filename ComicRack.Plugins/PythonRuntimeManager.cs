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
        /// Controls whether Python execution tracing is enabled.
        /// </summary>
        public static bool EnablePythonTracing
        {
            get => _enablePythonTracing;
            set
            {
                _enablePythonTracing = value;
                // Set environment variable so clr_bridge.py can check it
                Environment.SetEnvironmentVariable("COMICRACK_TRACE_ENABLED", value ? "true" : "false");
                if (Instance.IsInitialized)
                {
                    Instance.SetTrace(value);
                }
            }
        }
        private static bool _enablePythonTracing;

        public bool IsInitialized => _initialized;


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

                    // Check if module cache clear was requested before initialization
                    if (_pendingModuleCacheClear)
                    {
                        ClearPythonModuleCache();
                    }

                    SetTrace(EnablePythonTracing);
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
            AddSearchPath(AppDomain.CurrentDomain.BaseDirectory);
            AddSearchPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts"));
        }

        private void SetTrace(bool enable)
        {
            if (!_initialized) return;
            using (Py.GIL())
            {
                try
                {
                    // Use clr_bridge's trace functions for consistent behavior
                    try
                    {
                        dynamic bridge = Py.Import("clr_bridge");
                        if (enable)
                        {
                            bridge.enable_trace();
                            LogManager.Info("Python", "Execution tracing ENABLED via clr_bridge.");
                        }
                        else
                        {
                            bridge.disable_trace();
                            LogManager.Info("Python", "Execution tracing DISABLED via clr_bridge.");
                        }
                        return;
                    }
                    catch
                    {
                        // clr_bridge not available, fall back to direct trace setup
                    }

                    // Fallback: set trace directly
                    dynamic sys = Py.Import("sys");
                    if (enable)
                    {
                        string traceFuncCode = @"
import sys
def _trace_func(frame, event, arg):
    code = frame.f_code
    filename = code.co_filename
    if 'Scripts' in filename or (filename.endswith('.py') and '<' not in filename):
        lineno = frame.f_lineno
        name = code.co_name
        if event == 'call':
            print(f'[TRACE] CALL: {name}() at {filename}:{lineno}')
        elif event == 'line':
            print(f'[TRACE] LINE: {filename}:{lineno}')
        elif event == 'return':
            print(f'[TRACE] RETURN: {name}()')
        elif event == 'exception':
            exc_type, exc_value, exc_tb = arg
            print(f'[TRACE] EXCEPTION: {exc_type.__name__}: {exc_value}')
    return _trace_func
sys.settrace(_trace_func)
print('[TRACE] === Tracing ENABLED (fallback) ===')
";
                        PythonEngine.Exec(traceFuncCode);
                        LogManager.Info("Python", "Execution tracing ENABLED (fallback).");
                    }
                    else
                    {
                        sys.settrace(null);
                        LogManager.Info("Python", "Execution tracing DISABLED.");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Warning("Python", $"Failed to set trace: {ex.Message}");
                }
            }
        }

        private void ApplyTraceToScope(PyModule scope)
        {
            if (!_initialized || !EnablePythonTracing) return;
            
            try
            {
                // Define trace function within the scope
                // Output goes to stdout which is redirected to LogManager
                string traceFuncCode = @"
import sys

def _trace_func(frame, event, arg):
    if event == 'line':
        code = frame.f_code
        filename = code.co_filename
        lineno = frame.f_lineno
        
        # Only trace Python script files (not system libraries)
        if 'Scripts' in filename or filename.endswith('.py'):
            print(f'[TRACE] {filename}:{lineno}')
    
    return _trace_func

sys.settrace(_trace_func)
";
                scope.Exec(traceFuncCode);
                LogManager.Debug("Python", "Execution tracing applied to script scope.");
            }
            catch (Exception ex)
            {
                LogManager.Warning("Python", $"Failed to apply trace to scope: {ex.Message}");
            }
        }

        private PythonOutput _pythonOutput;

        private void EnsureOutputRedirection()
        {
            if (!_initialized) return;
            // No strict check for PythonCommand.Output == null here, just ensure redirection is active if initialized.
            // But we should respect if it's already set.

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    // Check if already redirected to avoid recursion or multiple wrappers
                    if (sys.stdout != null)
                    {
                        try
                        {
                            // Use Convert.ToPython to properly handle the conversion
                            var stdoutClass = sys.stdout.GetAttr("__class__");
                            if (stdoutClass != null)
                            {
                                var stdoutClassName = stdoutClass.GetAttr("__name__");
                                if (stdoutClassName != null && stdoutClassName.ToString() != "PythonOutput")
                                {
                                    _pythonOutput = new PythonOutput();
                                    sys.stdout = _pythonOutput.ToPython();
                                    sys.stderr = _pythonOutput.ToPython();

                                    // Python 3: print some verification
                                    LogManager.Debug("Python", "Redirection established.");
                                    PythonEngine.Exec("print('--- Python.NET Initialized ---')");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug("Python", $"Could not check/set redirection: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Warning("Python", $"EnsureOutputRedirection failed: {ex.Message}");
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

            LogManager.Info("System", "Beginning Python runtime shutdown...");

            var shutdownThread = new System.Threading.Thread(() =>
            {
                try
                {
                    if (!PythonEngine.IsInitialized)
                    {
                        try { _initialized = false; } catch { }
                        return;
                    }

                    // Do NOT attempt to restore thread state here. EndAllowThreads must be called 
                    // on the same thread that called BeginAllowThreads. Since we are on a background
                    // shutdown thread, we cannot restore the state of the original initialization thread.
                    // We will just proceed to acquire the GIL and shut down.
                    _threadState = IntPtr.Zero;

                    using (Py.GIL())
                    {
                        _api = null;

                        // Clear script cache
                        if (_scriptCache != null)
                        {
                            foreach (var scope in _scriptCache.Values)
                            {
                                try { scope?.Dispose(); } catch { }
                            }
                            _scriptCache.Clear();
                        }

                        if (_scope != null)
                        {
                            try { _scope.Dispose(); } catch { }
                            _scope = null;
                        }

                        // Clear sys.stdout/stderr and trace to break links to C# objects
                        try
                        {
                            dynamic sys = Py.Import("sys");
                            sys.settrace(null);
                            sys.stdout = sys.__stdout__;
                            sys.stderr = sys.__stderr__;
                        }
                        catch { }

                        _pythonOutput = null;
                    }

                    // Final engine shutdown
                    LogManager.Debug("System", "Calling PythonEngine.Shutdown...");
                    PythonEngine.Shutdown();
                    LogManager.Debug("System", "PythonEngine.Shutdown completed.");
                }
                catch (Exception ex)
                {
                    LogManager.Error("System", $"Error during Python shutdown: {ex.Message}");
                }
                finally
                {
                    _initialized = false;
                }
            });

            shutdownThread.IsBackground = true;
            shutdownThread.Start();
            
            // Wait for shutdown with a timeout
            if (!shutdownThread.Join(2000)) // 2 second timeout
            {
                LogManager.Warning("System", "Python runtime shutdown timed out. Proceeding with application exit.");
                // We leave the thread running (background) and let the process exit kill it
            }
            else
            {
                LogManager.Debug("System", "Python runtime shutdown complete.");
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
        private volatile bool _pendingModuleCacheClear = false;

        /// <summary>
        /// Clears the script cache, forcing scripts to be reloaded on next execution.
        /// This is needed when trace is enabled so cached scripts get trace applied.
        /// Also clears Python's sys.modules cache for script modules.
        /// </summary>
        public void ClearScriptCache()
        {
            if (_scriptCache != null)
            {
                // Dispose all cached scopes first
                foreach (var scope in _scriptCache.Values)
                {
                    try { scope?.Dispose(); } catch { }
                }
                _scriptCache.Clear();
                LogManager.Debug("Python", "Script cache cleared.");
            }

            // Also clear Python's module cache for script-related modules
            if (_initialized)
            {
                ClearPythonModuleCache();
            }
            else
            {
                // Mark for clearing when Python is next initialized/used
                _pendingModuleCacheClear = true;
                LogManager.Debug("Python", "Python module cache clear pending (engine not initialized).");
            }
        }

        private void ClearPythonModuleCache()
        {
            try
            {
                using (Py.GIL())
                {
                    // Clear cached script modules from sys.modules so they get reimported
                    string clearModulesCode = @"
import sys
modules_to_remove = [name for name in list(sys.modules.keys())
                     if 'Scripts' in str(getattr(sys.modules.get(name), '__file__', '') or '')
                     or name in ('clr_bridge', 'decorators')]
for name in modules_to_remove:
    try:
        del sys.modules[name]
    except:
        pass
print(f'[CACHE] Cleared {len(modules_to_remove)} Python modules from cache')
";
                    PythonEngine.Exec(clearModulesCode);
                    LogManager.Debug("Python", "Python module cache cleared for script modules.");
                }
            }
            catch (Exception ex)
            {
                LogManager.Warning("Python", $"Failed to clear Python module cache: {ex.Message}");
            }
            _pendingModuleCacheClear = false;
        }

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
            
            // 1. Extension Check (Global)
            if (!Path.GetExtension(scriptPath).Equals(".py", StringComparison.OrdinalIgnoreCase))
                throw new SecurityException($"Invalid script extension: {Path.GetExtension(scriptPath)}");

            // 2. Path Traversal Check
            if (!fullScriptPath.StartsWith(fullAllowedPath, StringComparison.OrdinalIgnoreCase) && 
                !fullScriptPath.Equals(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts")), StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityException($"Script path {scriptPath} is outside the allowed directory {fullAllowedPath}");
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

                // Re-apply tracing to the new scope if it was enabled
                if (EnablePythonTracing)
                {
                    ApplyTraceToScope(newScope);
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

            LogManager.Debug("Python", $"CallFunction: {functionName} (trace={EnablePythonTracing})");

            using (Py.GIL())
            {
                // If tracing is enabled, wrap the call in Python to get proper trace output
                if (EnablePythonTracing)
                {
                    LogManager.Debug("Python", $"Using traced execution for: {functionName}");
                    return CallFunctionWithTrace(functionName, args);
                }

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
                        return func.Invoke(pyArgs).AsManagedObject(typeof(object));
                    }
                    else
                    {
                        return func.Invoke().AsManagedObject(typeof(object));
                    }
                }
            }
        }

        /// <summary>
        /// Calls a function with Python-level tracing enabled.
        /// This wraps the call in Python code so sys.settrace works properly.
        /// </summary>
        private dynamic CallFunctionWithTrace(string functionName, object[] args)
        {
            try
            {
                // Build argument string for Python call
                var argNames = new System.Collections.Generic.List<string>();
                if (args != null && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string argName = $"_trace_arg_{i}";
                        argNames.Add(argName);
                        _scope.Set(argName, args[i].ToPython());
                    }
                }

                string argsStr = string.Join(", ", argNames);

                // Define the callback delegate
                Action<string, string> traceCallback = (source, message) => 
                {
                    // Directly pipe to LogManager
                     LogManager.Trace(source, message);
                };
                
                // Inject the callback into Python scope
                _scope.Set("_cr_trace_callback", traceCallback);

                // Build the traced call - all in Python so trace works
                string traceWrapper = $@"
import sys
import traceback
import System

# Safe representation of objects to prevent crashes during tracing
def _cr_safe_repr(obj):
    try:
        if obj is None:
            return 'None'
        # Handle .NET objects specifically if needed, but str() is usually safer than repr() for interop
        return str(obj)[:200] 
    except:
        return '<repr failed>'

# Trace function that calls back to C# directly
def _cr_trace_func(frame, event, arg):
    try:
        code = frame.f_code
        filename = code.co_filename
        
        # We only care about events in Scripts, .py files, or dynamic execution (<string>)
        # Exclude frozen/internal '<' files if necessary, but allow basic '<string>'
        if 'Scripts' in filename or filename.endswith('.py') or filename == '<string>':
            lineno = frame.f_lineno
            name = code.co_name
            
            message = ''
            if event == 'call':
                message = f'CALL: {{name}}() at {{filename}}:{{lineno}}'
            elif event == 'line':
                # Too noisy for now, keep line disabled unless needed, or enable just for debug
                message = f'LINE: {{filename}}:{{lineno}}'
            elif event == 'return':
                val = _cr_safe_repr(arg)
                message = f'RETURN: {{name}}() -> {{val}}'
            elif event == 'exception':
                exc_type, exc_value, exc_tb = arg
                message = f'EXCEPTION in {{name}}() at {{filename}}:{{lineno}}: {{exc_type.__name__}}: {{exc_value}}'
            
            # Invoke the C# callback
            if message:
                _cr_trace_callback('PythonTrace', message)
                
    except Exception as e:
        # If tracing itself fails, we try to log that fact but don't crash
        try:
            _cr_trace_callback('PythonTrace', f'Trace Internal Error: {{e}}')
        except:
            pass
            
    return _cr_trace_func

# Set trace and call function
_cr_result = None
_cr_error = None

_cr_trace_callback('System', '=== Starting traced execution of {functionName} ===')
sys.settrace(_cr_trace_func)

try:
    _cr_result = {functionName}({argsStr})
except Exception as e:
    _cr_error = e
    _cr_trace_callback('PythonError', f'Error during execution: {{e}}')
    # Log full traceback
    tb_lines = traceback.format_exception(type(e), e, e.__traceback__)
    for line in tb_lines:
        for sub_line in line.rstrip().split('\n'):
            if sub_line.strip():
                _cr_trace_callback('PythonError', sub_line)
finally:
    sys.settrace(None)
    _cr_trace_callback('System', '=== Trace ended ===')

if _cr_error:
    raise _cr_error
";

                _scope.Exec(traceWrapper);

                // Get result
                dynamic result = _scope.Get("_cr_result");

                // Clean up temp variables
                foreach (var argName in argNames)
                {
                    try { _scope.Exec($"del {argName}"); } catch { }
                }
                try { _scope.Exec("del _cr_result, _cr_error, _cr_trace_func, _cr_safe_repr, _cr_trace_callback"); } catch { }

                if (result != null)
                {
                    return ((PyObject)result).AsManagedObject(typeof(object));
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Warning("Python", $"Traced execution failed ({ex.Message}). Falling back to standard execution.");
                
                // Fallback to normal execution
                try
                {
                     dynamic sys = Py.Import("sys");
                     sys.settrace(null);
                }
                catch {}

                dynamic funcDyn = _scope.Get(functionName);
                using (PyObject func = funcDyn as PyObject)
                {
                    if (args != null && args.Length > 0)
                    {
                         var pyArgs = new PyObject[args.Length];
                         for (int i = 0; i < args.Length; i++) pyArgs[i] = args[i].ToPython();
                         return func.Invoke(pyArgs).AsManagedObject(typeof(object));
                    }
                    else
                    {
                         return func.Invoke().AsManagedObject(typeof(object));
                    }
                }
            }
        }
}
}


