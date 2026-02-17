using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cYo.Projects.ComicRack.Plugins.Bridge
{
    public class ApiIpcServer
    {
        private readonly string _pipeName;
        private CancellationTokenSource _cts;

        public event Func<IpcRequest, Task<IpcResponse>> RequestReceived;

        public ApiIpcServer(string pipeName = "ComicRackApiPipe")
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
                                    response = new IpcResponse { Success = false, Error = "No API request handler registered." };
                                }

                                string jsonResponse = JsonConvert.SerializeObject(response);
                                await writer.WriteLineAsync(jsonResponse);
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception)
                    {
                        // Silent in background
                    }
                }
            }
        }
    }
}
