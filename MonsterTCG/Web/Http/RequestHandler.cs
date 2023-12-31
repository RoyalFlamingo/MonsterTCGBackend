using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MonsterTCG.Http
{
	class RequestHandler
	{
		private TcpClient socket;
		private Server httpServer;
		public Request req;

		public RequestHandler(TcpClient s, Server httpServer)
		{
			this.socket = s ?? throw new ArgumentNullException(nameof(s));
			this.httpServer = httpServer ?? throw new ArgumentNullException(nameof(httpServer));
			this.req = new Request();
		}

		public async Task HandleRequestAsync()
		{
			using var writer = new StreamWriter(socket.GetStream()) { AutoFlush = true };
			using var reader = new StreamReader(socket.GetStream());

			// read request line
			string line;
			while ((line = await reader.ReadLineAsync()) != null)
			{
				if (string.IsNullOrEmpty(line))
					break;

				ProcessLine(line);
			}

			if (req.Headers.TryGetValue("Content-Length", out var contentLengthValue) && int.TryParse(contentLengthValue, out int contentLength))
			{
				if (contentLength > 0)
				{
					char[] buffer = new char[contentLength];
					int readLength = await reader.ReadBlockAsync(buffer, 0, contentLength);
					req.Body = new string(buffer, 0, readLength);
				}
			}

			Router router = new Router();
			Response res = await router.RouteRequestAsync(req);

			// send reponse
			await res.SendResponseAsync(writer);
		}

		public void ProcessLine(string line)
		{
			if (req.Method == null)
			{
				var requestLineParts = line.Split(' ');
				if (requestLineParts.Length >= 3)
				{
					req.Method = requestLineParts[0];    // z.B. "GET"
					req.Path = requestLineParts[1];      // z.B. "/home"
					req.Version = requestLineParts[2];   // z.B. "HTTP/1.1"
				}
			}
			else
			{
				var headerParts = line.Split(new[] { ':' }, 2);
				if (headerParts.Length == 2)
				{
					var headerName = headerParts[0].Trim();
					var headerValue = headerParts[1].Trim();
					req.Headers[headerName] = headerValue;
				}
			}
		}
	}
}
