using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cYo.Projects.ComicRack.Plugins.Bridge
{
    public class LegacyIpcClient : IDisposable
    {
        private readonly string _pipeName;
        private NamedPipeClientStream _pipeClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public LegacyIpcClient(string pipeName = "ComicRackLegacyPipe")
        {
            _pipeName = pipeName;
        }

        public async Task ConnectAsync(int timeoutMs = 5000)
        {
            _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await _pipeClient.ConnectAsync(timeoutMs);
            _reader = new StreamReader(_pipeClient);
            _writer = new StreamWriter(_pipeClient) { AutoFlush = true };
        }

        public async Task<IpcResponse> SendRequestAsync(IpcRequest request)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (_pipeClient == null || !_pipeClient.IsConnected)
                    throw new InvalidOperationException("IPC Client is not connected.");

                string jsonRequest = JsonConvert.SerializeObject(request);
                await _writer.WriteLineAsync(jsonRequest);

                string jsonResponse = await _reader.ReadLineAsync();
                if (jsonResponse == null) throw new IOException("Pipe connection closed by host.");

                return JsonConvert.DeserializeObject<IpcResponse>(jsonResponse);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _pipeClient?.Dispose();
        }
    }

    public class IpcRequest
    {
        public string Command { get; set; }
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
