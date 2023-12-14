using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MonsterTCG.Business.Models;
using Npgsql;

namespace MonsterTCG.Business.Database
{
	internal class PlayerRepository
	{
		public async Task<bool> CreatePlayer(string username, string hashedPassword)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			try
			{
				using (var connection = new NpgsqlConnection(connectionString))
				{
					await connection.OpenAsync();

					using (var command = new NpgsqlCommand("INSERT INTO players (accountname, password, name, coins, elo, wins, losses, bio, image, token) VALUES (@accountname, @password, @name, @coins, @elo, @wins, @losses, @bio, @image, @token)", connection))
					{
						command.Parameters.AddWithValue("@accountName", username);
						command.Parameters.AddWithValue("@password", hashedPassword);
						command.Parameters.AddWithValue("@name", username);
						command.Parameters.AddWithValue("@coins", 20);
						command.Parameters.AddWithValue("@elo", 1000);
						command.Parameters.AddWithValue("@wins", 0);
						command.Parameters.AddWithValue("@losses", 0);
						command.Parameters.AddWithValue("@bio", "");
						command.Parameters.AddWithValue("@image", ":)");
						command.Parameters.AddWithValue("@token", "");

						var result = await command.ExecuteNonQueryAsync();
						return result > 0;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<bool> UpdatePlayer(Player player)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			try
			{
				using (var connection = new NpgsqlConnection(connectionString))
				{
					await connection.OpenAsync();

					string sql = "UPDATE players SET accountname = @accountName, password = @password, name = @name, coins = @coins, elo = @elo, wins = @wins, losses = @losses, bio = @bio, image = @image, token = @token WHERE id = @id";
					using (var command = new NpgsqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@id", player.Id);
						command.Parameters.AddWithValue("@accountName", player.AccountName ?? "");
						command.Parameters.AddWithValue("@password", player.Password ?? "");
						command.Parameters.AddWithValue("@name", player.Name ?? "");
						command.Parameters.AddWithValue("@coins", player.Coins);
						command.Parameters.AddWithValue("@elo", player.Elo);
						command.Parameters.AddWithValue("@wins", player.Wins);
						command.Parameters.AddWithValue("@losses", player.Losses);
						command.Parameters.AddWithValue("@bio", player.Bio ?? "");
						command.Parameters.AddWithValue("@image", player.Image ?? "");
						command.Parameters.AddWithValue("@token", player.Token ?? "");

						var result = await command.ExecuteNonQueryAsync();
						return result > 0;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating player: {ex.Message}");
				return false;
			}
		}

		public async Task<Player> GetPlayerWithName(string username)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT * FROM players WHERE accountname = @username", connection))
				{
					command.Parameters.AddWithValue("@username", username);

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							Player player = ReadPlayerColumns(reader);
							return player;
						}
					}
				}
			}

			return null;
		}

		public async Task<Player> GetPlayer(string token)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT * FROM players WHERE token = @token", connection))
				{
					command.Parameters.AddWithValue("@token", token);

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							Player player = ReadPlayerColumns(reader);
							return player;
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Reads all player related data from the player table
		/// </summary>
		private static Player ReadPlayerColumns(NpgsqlDataReader reader)
		{
			return new Player
			{
				Id = reader.GetInt32(reader.GetOrdinal("id")),
				AccountName = reader.GetString(reader.GetOrdinal("accountname")),
				Password = reader.GetString(reader.GetOrdinal("password")),
				Name = reader.GetString(reader.GetOrdinal("name")),
				Wins = reader.GetInt32(reader.GetOrdinal("wins")),
				Losses = reader.GetInt32(reader.GetOrdinal("losses")),
				Elo = reader.GetInt32(reader.GetOrdinal("elo")),
				Bio = reader.GetString(reader.GetOrdinal("bio")),
				Image = reader.GetString(reader.GetOrdinal("image")),
				Coins = reader.GetInt32(reader.GetOrdinal("coins")),
				Token = reader.GetString(reader.GetOrdinal("token")),
			};
		}
	}
}
