using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace cYo.Common.Runtime
{
	public class SingleInstance : ISingleInstance, IDisposable
	{
		private readonly string name;
		private readonly Action<string[]> StartNew;
		private readonly Action<string[]> StartLast;
		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		public SingleInstance(string name, Action<string[]> startNew, Action<string[]> startLast)
		{
			this.name = name;
			StartNew = startNew;
			StartLast = startLast;
		}

		public void Run(string[] args)
		{
			// Try to connect to existing instance
			if (TryNotifyExistingInstance(args))
			{
				return;
			}

			// No existing instance found, start new one
			StartServer();

			try
			{
				StartNew(args);
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Failed to start Program: " + ex.Message);
			}
		}

		private bool TryNotifyExistingInstance(string[] args)
		{
			try
			{
				using (var client = new NamedPipeClientStream(".", name, PipeDirection.Out))
				{
					client.Connect(500); // Wait 500ms for connection
					using (var writer = new StreamWriter(client))
					{
						writer.WriteLine(string.Join("\n", args)); // Simple serialization: Join with newline
					}
				}
				return true;
			}
			catch (TimeoutException)
			{
				return false; // No server listening
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void StartServer()
		{
			Task.Run(async () =>
			{
				while (!cancellationTokenSource.IsCancellationRequested)
				{
					try
					{
						using (var server = new NamedPipeServerStream(name, PipeDirection.In))
						{
                           await server.WaitForConnectionAsync(cancellationTokenSource.Token);
						   
                           using (var reader = new StreamReader(server))
                           {
                               string content = await reader.ReadToEndAsync();
                               var args = content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                               InvokeLast(args);
                           }
						}
					}
					catch (OperationCanceledException)
					{
						break;
					}
					catch (Exception ex)
					{
						Trace.WriteLine("SingleInstance server error: " + ex);
					}
				}
			});
		}

		public void InvokeLast(string[] args)
		{
			// Marshal to UI thread if necessary?
            // The original implementation called StartLast directly from WCF thread.
            // Assuming StartLast handles marshalling if needed (usually it invokes on form).
			StartLast?.Invoke(args);
		}

		public void InvokeNew(string[] args)
		{
			StartNew?.Invoke(args);
		}

		public void Dispose()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
		}
	}
}
