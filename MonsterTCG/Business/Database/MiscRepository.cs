using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using MonsterTCG.Business.Enums;
using MonsterTCG.Business.Models;
using Npgsql;
using MonsterTCG.Config;

namespace MonsterTCG.Business.Database
{
	internal class MiscRepository
	{
		public async Task InsertMigration(int id)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			try
			{
				using (var connection = new NpgsqlConnection(connectionString))
				{
					await connection.OpenAsync();

					using (var command = new NpgsqlCommand("INSERT INTO migrations (id) VALUES (@id)", connection))
					{
						command.Parameters.AddWithValue("@id", id);

						var result = await command.ExecuteNonQueryAsync();
						return;
					}
				}
			}
			catch (Exception)
			{
				return;
			}
		}
	}
}
