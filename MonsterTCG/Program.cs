using MonsterTCG.Business.Database;
using MonsterTCG.Http;
using MonsterTCG.Config;
using System.Reflection;
using MonsterTCG.Business.Migrations;
using Npgsql;

namespace MonsterTCG
{
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				ConfigurationManager.LoadConfiguration();

				if (ConfigurationManager.DeleteTablesOnStartup)
				{
					Console.WriteLine("Do you want to delete all tables? (y/n): ");
					if (Console.ReadKey().KeyChar != 'Y')
						DatabaseSetup.DeleteTables();
				}

				DatabaseSetup.CreateTables();

				RunMigrations();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error loading configuration or connecting to the database:");
				Console.WriteLine(ex.ToString());
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


		private static void RunMigrations()
		{
			var migrations = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.IsClass && t.Namespace == "MonsterTCG.Business.Migrations"
							&& typeof(IMigration).IsAssignableFrom(t)); // Check if it implements IMigration

			foreach (var migrationType in migrations)
			{
				var migrationInstance = Activator.CreateInstance(migrationType) as IMigration;
				if (migrationInstance == null)
					continue;

				int version = migrationInstance.Version;
				try
				{
					using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionString))
					{
						conn.Open();

						var cmd = new NpgsqlCommand("SELECT id FROM migrations WHERE id = @id", conn);
						cmd.Parameters.AddWithValue("@id", version);
						using (var reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								Console.WriteLine($"Migration {version} skipped.");
								continue;
							}
						}
					}

					var method = migrationType.GetMethod("Up");
					if (method != null)
					{
						Console.WriteLine($"Executing migration: {migrationType.Name}");
						method.Invoke(migrationInstance, null);
					}

					using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionString))
					{
						conn.Open();

						var cmd = new NpgsqlCommand("INSERT INTO migrations (id) VALUES (@id)", conn);
						cmd.Parameters.AddWithValue("@id", version);
						cmd.ExecuteNonQuery();
					}

				}
				catch (Exception ex)
				{
					Console.WriteLine($"Migration error: {ex.Message}");
				}
			}
		}
	}
}

