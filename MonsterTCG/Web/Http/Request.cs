using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonsterTCG.Http
{
	public class Request
	{
		public string Method { get; set; }
		public string Path { get; set; }
		public string Version { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public string Body { get; set; }

		public Request()
		{
			Headers = new Dictionary<string, string>();
		}

		public void AddHeader(string key, string value)
		{
			Headers[key] = value;
		}
	}
}