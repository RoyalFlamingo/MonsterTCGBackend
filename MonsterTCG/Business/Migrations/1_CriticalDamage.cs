using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTCG.Config;
using System.Transactions;
using MonsterTCG.Business.Models;

namespace MonsterTCG.Business.Migrations
{
	class _1_CriticalDamage : IMigration
	{
		int IMigration.Version => 1;

		public void Up()
		{
			using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionString))
			{
				conn.Open();

				var cmd2 = new NpgsqlCommand("ALTER TABLE cards ADD COLUMN critchance DOUBLE PRECISION DEFAULT 0.1", conn).ExecuteNonQuery;
				var cmd3 = new NpgsqlCommand("UPDATE cards SET critchance = 0.1", conn).ExecuteNonQuery;
			}
		}


	}
}
