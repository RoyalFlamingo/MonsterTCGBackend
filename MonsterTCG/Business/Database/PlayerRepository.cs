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

		public async Task<List<PlayerStats>> GetScoreboard()
		{
			string connectionString = ConfigurationManager.ConnectionString;
			List<PlayerStats> players = new List<PlayerStats>();

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT name, elo, wins, losses FROM players ORDER BY elo DESC", connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							PlayerStats playerStats = new PlayerStats();
							playerStats.Name = reader.GetString(reader.GetOrdinal("name"));
							playerStats.Elo = reader.GetInt32(reader.GetOrdinal("elo"));
							playerStats.Wins = reader.GetInt32(reader.GetOrdinal("wins"));
							playerStats.Losses = reader.GetInt32(reader.GetOrdinal("losses"));

							players.Add(playerStats);
						}
					}
				}
			}

			return players;
		}

		public async Task<List<TradingDeal>> GetTradingDeals()
		{
			string connectionString = ConfigurationManager.ConnectionString;
			List<TradingDeal> deals = new List<TradingDeal>();

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT id, card_guid, type, mindamage FROM tradingdeals", connection))
				{
					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							TradingDeal deal = new TradingDeal();
							deal.Id = reader.GetGuid(reader.GetOrdinal("id")).ToString();
							deal.CardToTrade = reader.GetGuid(reader.GetOrdinal("card_guid")).ToString();
							deal.Type = ParseCardType(reader.GetString(reader.GetOrdinal("type")));
							deal.MinimumDamage = reader.GetInt32(reader.GetOrdinal("mindamage"));

							deals.Add(deal);
						}
					}
				}
			}

			return deals;
		}

		public async Task<bool> CreateTradingDeal(TradingDeal deal)
		{
			string connectionString = ConfigurationManager.ConnectionString;


			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				var sql = "SELECT owner_id FROM stacks WHERE owner_id = @owner AND card_guid = @guid";
				using (var command = new NpgsqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@owner", deal.OwnerId);
					command.Parameters.AddWithValue("@guid", Guid.Parse(deal.CardToTrade));

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (!await reader.ReadAsync()) // checks if a card was found
						{
							return false;
						}
					}
				}

				sql = "SELECT card_guid FROM decks WHERE card_guid = @card";
				using (var command = new NpgsqlCommand(sql, connection))
				{
					command.Parameters.AddWithValue("@card", Guid.Parse(deal.CardToTrade));

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync()) // checks if a card was found
						{
							return false;
						}
					}
				}

				using (var command = new NpgsqlCommand("INSERT INTO tradingdeals (id, card_guid, type, owner_id, mindamage) VALUES (@id, @card, @type, @owner, @dmg)", connection))
				{
					command.Parameters.AddWithValue("@id", Guid.Parse(deal.Id));
					command.Parameters.AddWithValue("@card", Guid.Parse(deal.CardToTrade));
					command.Parameters.AddWithValue("@type", deal.Type.ToString());
					command.Parameters.AddWithValue("@owner", deal.OwnerId);
					command.Parameters.AddWithValue("@dmg", deal.MinimumDamage);

					var result = await command.ExecuteNonQueryAsync();
					return result > 0;
				}
			}

		}

		public async Task<bool> DeleteTradingDeal(string dealId)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("DELETE FROM tradingdeals WHERE id = @dealid", connection))
				{
					command.Parameters.AddWithValue("@dealid", Guid.Parse(dealId));

					var result = await command.ExecuteNonQueryAsync();
					return result > 0;
				}
			}

		}

		public async Task<bool> Trade(Card cardTradeDeal, Card offeredCard, int traderPlayerId, int offeringPlayerId, string tradeId)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					var sql = "DELETE FROM tradingdeals WHERE id = @trade";
					using (var command = new NpgsqlCommand(sql, connection, transaction))
					{
						command.Parameters.AddWithValue("@trade", Guid.Parse(tradeId));

						if(await command.ExecuteNonQueryAsync() == 0)
						{
							await transaction.RollbackAsync();
							return false;
						}
					}

					sql = "UPDATE stacks SET owner_id = @newowner WHERE card_guid = @cardguid";
					using (var command = new NpgsqlCommand(sql, connection, transaction))
					{
						command.Parameters.AddWithValue("@cardguid", Guid.Parse(cardTradeDeal.Guid));
						command.Parameters.AddWithValue("@newowner", offeringPlayerId);

						if (await command.ExecuteNonQueryAsync() == 0)
						{
							await transaction.RollbackAsync();
							return false;
						}
					}

					sql = "UPDATE stacks SET owner_id = @newowner WHERE card_guid = @cardguid";
					using (var command = new NpgsqlCommand(sql, connection, transaction))
					{
						command.Parameters.AddWithValue("@cardguid", Guid.Parse(offeredCard.Guid));
						command.Parameters.AddWithValue("@newowner", traderPlayerId);

						if (await command.ExecuteNonQueryAsync() == 0)
						{
							await transaction.RollbackAsync();
							return false;
						}
					}

					await transaction.CommitAsync();
					return true;
				}
			}
		}

		public async Task ChangeCardOwnership(Card card, int newOwner)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					var sql = "UPDATE stacks SET owner_id = @newowner WHERE card_guid = @cardguid";
					using (var command = new NpgsqlCommand(sql, connection, transaction))
					{
						command.Parameters.AddWithValue("@cardguid", Guid.Parse(card.Guid));
						command.Parameters.AddWithValue("@newowner", newOwner);

						if (await command.ExecuteNonQueryAsync() == 0)
						{
							await transaction.RollbackAsync();
							return;
						}
					}

					await transaction.CommitAsync();
					return;
				}
			}
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

		private static CardType ParseCardType(string type)
		{
			return (CardType)Enum.Parse(typeof(CardType), type);
		}
	}
}
