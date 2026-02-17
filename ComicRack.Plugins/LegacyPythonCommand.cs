using System;
using System.IO;
using cYo.Projects.ComicRack.Plugins.Bridge;

namespace cYo.Projects.ComicRack.Plugins
{
    public class LegacyPythonCommand : Command
    {
        public string ScriptFile { get; set; }
        public string Method { get; set; }

        private static readonly LegacyHostManager _hostManager = new LegacyHostManager();

        protected override object OnInvoke(object[] data)
        {
            try
            {
                LogManager.Info("Script", $"Invoking legacy script: {ScriptFile}");
                
                // Set the API target for the back-channel
                _hostManager.SetApiTarget(Environment);
                
                // Get a connected client
                var client = _hostManager.GetClientAsync().GetAwaiter().GetResult();
                
                // Send execution request
                var request = new IpcRequest
                {
                    Command = "Execute",
                    ScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ScriptFile),
                    MethodName = Method,
                    Arguments = data
                };

                var response = client.SendRequestAsync(request).GetAwaiter().GetResult();
                
                if (!response.Success)
                {
                    LogManager.Error("Script", $"Legacy Host Error: {response.Error}");
                    return null;
                }

                return response.Result;
            }
            catch (Exception ex)
            {
                LogManager.Error("Script", $"Failed to invoke legacy script: {ex.Message}");
                return null;
            }
        }
    }
}
