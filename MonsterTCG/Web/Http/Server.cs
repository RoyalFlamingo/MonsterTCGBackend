using MonsterTCG.Business.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MonsterTCG.Http
{
	class Server
	{
		protected int port;
		TcpListener listener;
		CancellationTokenSource cancellationTokenSource;
		
		public Server(int port)
		{
			this.port = port;
			listener = new TcpListener(IPAddress.Loopback, port);
			cancellationTokenSource = new CancellationTokenSource();
			
		}

		public async Task RunAsync()
		{
			listener.Start();
			Console.WriteLine($"Server listening on port {port}...");

			try
			{
				while (!cancellationTokenSource.Token.IsCancellationRequested)
				{
					var acceptTask = listener.AcceptTcpClientAsync();
					if (await Task.WhenAny(acceptTask, Task.Delay(-1, cancellationTokenSource.Token)) == acceptTask)
					{
						TcpClient client = await acceptTask;
						RequestHandler handler = new RequestHandler(client, this);

						var processTask = Task.Run(() => handler.HandleRequestAsync(), cancellationTokenSource.Token);
						try
						{
							await processTask;
						}
						catch (Exception ex)
						{
							Console.WriteLine($"An exception occurred while processing the request: {ex.Message}");
						}
					}
					else
					{
						break;
					}
				}
			}

			catch (OperationCanceledException)
			{
				Console.WriteLine("Server closing...");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Server exception: {ex.Message}");
			}
			finally
			{
				listener.Stop();
			}
		}

		public void Stop()
		{
			cancellationTokenSource.Cancel();
		}
	}
}
