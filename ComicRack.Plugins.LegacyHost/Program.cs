using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using cYo.Projects.ComicRack.Plugins.LegacyHost.Bridge;

namespace cYo.Projects.ComicRack.Plugins.LegacyHost
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            string pipeName = args.FirstOrDefault(a => a.StartsWith("--pipe="))?.Split('=')[1] ?? "ComicRackLegacyPipe";
            string apiPipeName = args.FirstOrDefault(a => a.StartsWith("--api-pipe="))?.Split('=')[1] ?? "ComicRackApiPipe";
            Console.WriteLine($"Legacy IronPython 2.7.4 Host Started. Pipe: {pipeName}, ApiPipe: {apiPipeName}");
            
            var apiClient = new ApiIpcClient(apiPipeName);
            // We connect to the API pipe asynchronously but wait for it before starting the engine if possible
            apiClient.ConnectAsync().ContinueWith(t => 
            {
                if (t.IsFaulted) Console.WriteLine("Failed to connect to API Pipe: " + t.Exception.Message);
                else Console.WriteLine("Connected to API Pipe.");
            });
            LyoutProxy.SetApiClient(apiClient);

            var engineHost = new IronPythonEngineHost();
            var server = new IpcServer(pipeName);

            server.RequestReceived += async (req) =>
            {
                try
                {
                    if (req.Command == "Execute")
                    {
                        var result = engineHost.ExecuteMethod(req.ScriptPath, req.MethodName, req.Arguments);
                        return new IpcResponse { Success = true, Result = result };
                    }
                    if (req.Command == "Shutdown")
                    {
                        Application.Exit();
                        return new IpcResponse { Success = true };
                    }
                    return new IpcResponse { Success = false, Error = "Unknown command: " + req.Command };
                }
                catch (Exception ex)
                {
                    return new IpcResponse { Success = false, Error = ex.Message };
                }
            };

            server.Start();
            
            if (args.Contains("--test"))
            {
                RunTest(engineHost);
            }
            
            Application.Run();
            server.Stop();
        }

        static void RunTest(IronPythonEngineHost host)
        {
            // Placeholder for test logic
            Console.WriteLine("Test mode active.");
        }
    }
}
