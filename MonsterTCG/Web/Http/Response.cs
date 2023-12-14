using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Http
{
	public class Response
	{
		public HttpStatusCode StatusCode { get; set; }
		public Dictionary<string, string> Headers { get; private set; }
		public string Content { get; set; }

		public Response()
		{
			StatusCode = HttpStatusCode.OK;
			Headers = new Dictionary<string, string>();
		}

		public void AddHeader(string key, string value)
		{
			Headers[key] = value;
		}

		public async Task SendResponseAsync(StreamWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			// Senden des Statuscodes und der Headers
			await writer.WriteLineAsync($"HTTP/1.1 {(int)StatusCode} {StatusCode}");
			foreach (var header in Headers)
			{
				await writer.WriteLineAsync($"{header.Key}: {header.Value}");
			}
			await writer.WriteLineAsync();

			// Senden des Body
			if (!string.IsNullOrEmpty(Content))
			{
				await writer.WriteLineAsync(Content);
			}
		}
	}
}