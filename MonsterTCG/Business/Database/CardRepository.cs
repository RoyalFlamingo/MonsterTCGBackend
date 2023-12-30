using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MonsterTCG.Business.Enums;
using MonsterTCG.Business.Models;
using MonsterTCG.Config;
using Npgsql;

namespace MonsterTCG.Business.Database
{
	class CardRepository
	{
		public async Task<bool> CreatePackage(IEnumerable<Card> cards)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					foreach (var card in cards)
					{
						var sql = "INSERT INTO cards (guid, name, type, element, damage) VALUES (@guid, @name, @type, @element, @damage)";
						using (var command = new NpgsqlCommand(sql, connection, transaction))
						{
							command.Parameters.AddWithValue("@guid", Guid.Parse(card.Guid));
							command.Parameters.AddWithValue("@name", card.Name);
							command.Parameters.AddWithValue("@type", card.Type.ToString());
							command.Parameters.AddWithValue("@element", card.Element.ToString());
							command.Parameters.AddWithValue("@damage", card.Damage);

							await command.ExecuteNonQueryAsync();
						}
					}

					await transaction.CommitAsync();
					return true;
				}
			}
		}

		public async Task<bool> SetDeck(IEnumerable<string> cardIds, int playerid)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				var sql = "";

				await connection.OpenAsync();
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						//check if any card is in a trading deal
						foreach (var cardId in cardIds)
						{
							sql = "SELECT card_guid FROM tradingdeals WHERE card_guid = @guid";
							using (var command = new NpgsqlCommand(sql, connection, transaction))
							{
								command.Parameters.AddWithValue("@guid", Guid.Parse(cardId));

								using (var reader = await command.ExecuteReaderAsync())
								{
									if (await reader.ReadAsync()) // checks if a card was found
									{
										await transaction.RollbackAsync();
										return false;
									}
								}
							}
						}

						//check if the provided cards belong to the player
						foreach (var cardId in cardIds)
						{
							sql = "SELECT owner_id FROM stacks WHERE owner_id = @owner AND card_guid = @guid";
							using (var command = new NpgsqlCommand(sql, connection, transaction))
							{
								command.Parameters.AddWithValue("@owner", playerid);
								command.Parameters.AddWithValue("@guid", Guid.Parse(cardId));

								using (var reader = await command.ExecuteReaderAsync())
								{
									if (!await reader.ReadAsync()) // checks if a card was found
									{
										await transaction.RollbackAsync();
										return false;
									}
								}
							}
						}

						//first reset the player deck, then add the new cards to the deck

						sql = "DELETE FROM decks WHERE owner_id = @owner";
						using (var command = new NpgsqlCommand(sql, connection, transaction))
						{
							command.Parameters.AddWithValue("@owner", playerid);

							await command.ExecuteNonQueryAsync();
						}

						foreach (var cardId in cardIds)
						{
							sql = "INSERT INTO decks (card_guid, owner_id) VALUES (@guid, @owner)";
							using (var command = new NpgsqlCommand(sql, connection, transaction))
							{
								command.Parameters.AddWithValue("@guid", Guid.Parse(cardId));
								command.Parameters.AddWithValue("@owner", playerid);

								await command.ExecuteNonQueryAsync();
							}
						}

						await transaction.CommitAsync();
						return true;

					}
					catch (Exception) // if an exception occurs, the transaction is rolled back
					{
						await transaction.RollbackAsync();
						return false;
					}
				}
			}
		}

		public async Task<bool> BuyPackage(Player player)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			try
			{
				using (var connection = new NpgsqlConnection(connectionString))
				{
					await connection.OpenAsync();

					var selectSql = @"
					SELECT guid 
					FROM cards 
					WHERE guid NOT IN (SELECT card_guid FROM stacks) 
					LIMIT 5";

					var cardsToInsert = new List<Guid>();

					using (var selectCommand = new NpgsqlCommand(selectSql, connection))
					{
						using (var reader = await selectCommand.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								cardsToInsert.Add(reader.GetGuid(0));
							}
						}
					}

					if (cardsToInsert.Count < 5)
						return false;

					foreach (var guid in cardsToInsert)
					{
						var insertSql = "INSERT INTO stacks (card_guid, owner_id) VALUES (@guid, @owner)";
						using (var insertCommand = new NpgsqlCommand(insertSql, connection))
						{
							insertCommand.Parameters.AddWithValue("@guid", guid);
							insertCommand.Parameters.AddWithValue("@owner", player.Id);
							await insertCommand.ExecuteNonQueryAsync();
						}
					}

					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<List<Card>> GetStack(int playerid)
		{
			string connectionString = ConfigurationManager.ConnectionString;
			List<Card> cards = new List<Card>();

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT c.guid, c.name, c.type, c.element, c.damage FROM stacks JOIN cards c ON card_guid = c.guid WHERE owner_id = @owner", connection))
				{
					command.Parameters.AddWithValue("@owner", playerid);

					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							Card card = ReadCardColumns(reader);
							cards.Add(card);
						}
					}
				}
			}

			return cards;
		}

		public async Task<List<Card>> GetDeck(int playerid)
		{
			string connectionString = ConfigurationManager.ConnectionString;
			List<Card> cards = new List<Card>();

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT c.guid, c.name, c.type, c.element, c.damage FROM decks JOIN cards c ON card_guid = c.guid WHERE owner_id = @owner", connection))
				{
					command.Parameters.AddWithValue("@owner", playerid);

					using (var reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							Card card = ReadCardColumns(reader);
							cards.Add(card);
						}
					}
				}
			}

			return cards;
		}

		public async Task<Card> GetCard(string cardGuid)
		{
			string connectionString = ConfigurationManager.ConnectionString;

			using (var connection = new NpgsqlConnection(connectionString))
			{
				await connection.OpenAsync();

				using (var command = new NpgsqlCommand("SELECT guid, name, type, element, damage FROM cards c WHERE guid = @guid", connection))
				{
					command.Parameters.AddWithValue("@guid", Guid.Parse(cardGuid));

					using (var reader = await command.ExecuteReaderAsync())
					{
						if(await reader.ReadAsync())
						{
							Card card = ReadCardColumns(reader);
							return card;
						}
						return null; //no card with given id was found
					}
				}
			}
		}

		private static Card ReadCardColumns(NpgsqlDataReader reader)
		{
			return new Card
			{
				Guid = reader.IsDBNull(reader.GetOrdinal("guid")) ? null : reader.GetGuid(reader.GetOrdinal("guid")).ToString(),
				Name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader.GetString(reader.GetOrdinal("name")),
				Type = ParseCardType(reader.GetString(reader.GetOrdinal("type"))),
				Element = ParseCardElement(reader.GetString(reader.GetOrdinal("element"))),
				Damage = reader.GetInt32(reader.GetOrdinal("damage"))
			};
		}

		private static CardType ParseCardType(string type)
		{
			return (CardType)Enum.Parse(typeof(CardType), type);
		}

		private static CardElement ParseCardElement(string element)
		{
			return (CardElement)Enum.Parse(typeof(CardElement), element);
		}
	}
}

