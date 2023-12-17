using MonsterTCG.Business.Database;
using MonsterTCG.Http;
using MonsterTCG.Config;

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
				Console.WriteLine("Error loading configuration or connecting to the database, closing...");
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
