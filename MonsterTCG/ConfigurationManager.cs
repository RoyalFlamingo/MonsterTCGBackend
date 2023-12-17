using Microsoft.Extensions.Configuration;
using System.IO;

namespace MonsterTCG.Config
{
	public static class ConfigurationManager
	{
		public static string ConnectionString { get; private set; }
		public static bool UseFakeTokens { get; private set; }
		public static int EloKFactor { get; private set; }

        public static void LoadConfiguration()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			IConfiguration configuration = builder.Build();
			ConnectionString = configuration.GetConnectionString("DefaultConnection");
			UseFakeTokens = configuration.GetValue<bool>("Settings:UseFakeTokens");
			EloKFactor = configuration.GetValue<int>("Settings:EloKFactor");
		}
	}
}
