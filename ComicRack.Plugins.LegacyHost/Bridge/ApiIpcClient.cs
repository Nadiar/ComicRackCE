using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cYo.Projects.ComicRack.Plugins.LegacyHost.Bridge
{
    public class ApiIpcClient : IDisposable
    {
        private readonly string _pipeName;
        private NamedPipeClientStream _pipeClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public ApiIpcClient(string pipeName = "ComicRackApiPipe")
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
                    throw new InvalidOperationException("API Pipe is not connected.");

                string jsonRequest = JsonConvert.SerializeObject(request);
                await _writer.WriteLineAsync(jsonRequest);

                string jsonResponse = await _reader.ReadLineAsync();
                if (jsonResponse == null) throw new IOException("API Pipe closed by host.");

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
}
