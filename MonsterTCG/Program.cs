using MonsterTCG.Business.Database;
using MonsterTCG.Http;

namespace MonsterTCG
{
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				ConfigurationManager.LoadConfiguration();
				DatabaseSetup dbSetup = new DatabaseSetup();
			}
			catch (Exception)
			{
				Console.WriteLine("Error setting up the server, closing...");
				return;
			}

			Server server = new Server(10001);
			var serverTask = server.RunAsync();

			Console.WriteLine("Press 'q' to close the server...");

			while (Console.ReadKey().KeyChar != 'q')
			{
				Console.WriteLine("Press 'q' to close the server...");
			}

			server.Stop();
			await serverTask;
		}
	}
}
