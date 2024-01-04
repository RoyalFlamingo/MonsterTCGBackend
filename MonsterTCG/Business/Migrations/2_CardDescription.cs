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
	class _2_CardDescription : IMigration
	{
		int IMigration.Version => 2;

		public void Up()
		{
			using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionString))
			{
				conn.Open();

				var cmd = new NpgsqlCommand("ALTER TABLE cards ADD COLUMN description VARCHAR(255) DEFAULT 'NO CARD DESCRIPTION'", conn).ExecuteNonQuery;
			}
		}
	}
}
