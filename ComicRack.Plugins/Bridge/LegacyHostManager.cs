using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cYo.Projects.ComicRack.Plugins.Bridge
{
    public class LegacyHostManager
    {
        private Process _hostProcess;
        private LegacyIpcClient _client;
        private ApiIpcServer _apiServer;
        private readonly string _hostPath;
        private object _apiTarget;

        public LegacyHostManager()
        {
            // Path to the sidecar executable
            _hostPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ComicRack.Plugins.LegacyHost.exe");
            
            _apiServer = new ApiIpcServer();
            _apiServer.RequestReceived += HandleApiRequest;
            _apiServer.Start();
        }

        public void SetApiTarget(object api)
        {
            _apiTarget = api;
        }

        private async Task<IpcResponse> HandleApiRequest(IpcRequest req)
        {
            try
            {
                if (_apiTarget == null) return new IpcResponse { Success = false, Error = "API Target not set." };

                object current = _apiTarget;
                string[] parts = req.MethodName.Split('.');
                
                // Skip the "ComicRack" root if it's there
                int start = parts[0] == "ComicRack" ? 1 : 0;

                for (int i = start; i < parts.Length; i++)
                {
                    string part = parts[i];
                    var type = current.GetType();
                    
                    // Try property first
                    var prop = type.GetProperty(part);
                    if (prop != null)
                    {
                        current = prop.GetValue(current);
                        continue;
                    }

                    // Try field
                    var field = type.GetField(part);
                    if (field != null)
                    {
                        current = field.GetValue(current);
                        continue;
                    }
                    
                    // If we are at the last part and it's an InvokeMember, we'll handle it below
                    if (i == parts.Length - 1 && req.Command == "InvokeMember") break;

                    return new IpcResponse { Success = false, Error = $"Member {part} not found on {type.Name}" };
                }

                if (req.Command == "GetMember")
                {
                    if (IsPrimitive(current)) return new IpcResponse { Success = true, Result = current };
                    return new IpcResponse { Success = true, Result = "__PROXY__:" + req.MethodName };
                }

                if (req.Command == "InvokeMember")
                {
                    string methodName = parts.Last();
                    // Basic method invocation (needs better overload handling for high fidelity)
                    var method = current.GetType().GetMethod(methodName);
                    if (method == null) return new IpcResponse { Success = false, Error = $"Method {methodName} not found." };

                    var result = method.Invoke(current, req.Arguments);
                    if (IsPrimitive(result)) return new IpcResponse { Success = true, Result = result };
                    return new IpcResponse { Success = true, Result = "__PROXY__:Result" };
                }

                return new IpcResponse { Success = false, Error = "Unknown API command: " + req.Command };
            }
            catch (Exception ex)
            {
                return new IpcResponse { Success = false, Error = ex.Message };
            }
        }

        private bool IsPrimitive(object obj)
        {
            if (obj == null) return true;
            var type = obj.GetType();
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }

        public async Task<LegacyIpcClient> GetClientAsync()
        {
            if (_client != null) return _client;

            if (_hostProcess == null || _hostProcess.HasExited)
            {
                StartHost();
            }

            _client = new LegacyIpcClient();
            await _client.ConnectAsync();
            return _client;
        }

        private void StartHost()
        {
            if (!File.Exists(_hostPath))
                throw new FileNotFoundException($"Legacy Host not found at {_hostPath}");

            var startInfo = new ProcessStartInfo
            {
                FileName = _hostPath,
                Arguments = "--pipe=ComicRackLegacyPipe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _hostProcess = Process.Start(startInfo);
            // Handle logs...
        }

        public void StopHost()
        {
            _client?.Dispose();
            _client = null;
            _apiServer?.Stop();
            if (_hostProcess != null && !_hostProcess.HasExited)
            {
                _hostProcess.Kill();
            }
        }
    }
}
