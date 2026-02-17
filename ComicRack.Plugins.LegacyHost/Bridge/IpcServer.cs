using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cYo.Projects.ComicRack.Plugins.LegacyHost.Bridge
{
    public class IpcServer
    {
        private readonly string _pipeName;
        private CancellationTokenSource _cts;

        public event Func<IpcRequest, Task<IpcResponse>> RequestReceived;

        public IpcServer(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    try
                    {
                        await pipeServer.WaitForConnectionAsync(token);
                        using (var reader = new StreamReader(pipeServer))
                        using (var writer = new StreamWriter(pipeServer) { AutoFlush = true })
                        {
                            while (!token.IsCancellationRequested && pipeServer.IsConnected)
                            {
                                string line = await reader.ReadLineAsync();
                                if (line == null) break;

                                var request = JsonConvert.DeserializeObject<IpcRequest>(line);
                                IpcResponse response;

                                if (RequestReceived != null)
                                {
                                    response = await RequestReceived(request);
                                }
                                else
                                {
                                    response = new IpcResponse { Success = false, Error = "No request handler registered." };
                                }

                                string jsonResponse = JsonConvert.SerializeObject(response);
                                await writer.WriteLineAsync(jsonResponse);
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Console.WriteLine("IPC Server Error: " + ex.Message);
                    }
                }
            }
        }
    }

    public class IpcRequest
    {
        public string Command { get; set; } // "Execute", "GetMetadata", "Shutdown"
        public string ScriptPath { get; set; }
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
    }

    public class IpcResponse
    {
        public bool Success { get; set; }
        public object Result { get; set; }
        public string Error { get; set; }
    }
}
