using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTCG.Config;

namespace MonsterTCG.Business.Database
{
	class DatabaseSetup
	{
        public DatabaseSetup()
        {
			using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionString))
			{
				conn.Open();

				// CREATE TABLE PLAYER
				var createPlayerTableCmd = new NpgsqlCommand(
					"CREATE TABLE IF NOT EXISTS players (" +
					"id SERIAL PRIMARY KEY, " +
					"accountname VARCHAR(50) UNIQUE, " +
					"password VARCHAR(100), " +
					"name VARCHAR(50), " +
					"coins INT, " +
					"elo INT, " +
					"wins INT, " +
					"losses INT, " +
					"bio VARCHAR(255), " +
					"image VARCHAR(255), " +
					"token VARCHAR(255))", conn);
				createPlayerTableCmd.ExecuteNonQuery();

				// CREATE TABLE STACK
				var createStackTableCmd = new NpgsqlCommand(
					"CREATE TABLE IF NOT EXISTS stacks (" +
					"id SERIAL PRIMARY KEY, " +
					"card_guid UUID, " +
					"owner_id INT REFERENCES players(id))", conn);
				createStackTableCmd.ExecuteNonQuery();

				// CREATE TABLE DECK
				var createDeckTableCmd = new NpgsqlCommand(
					"CREATE TABLE IF NOT EXISTS decks (" +
					"id SERIAL PRIMARY KEY, " +
					"card_guid UUID, " +
					"owner_id INT REFERENCES players(id))", conn);
				createDeckTableCmd.ExecuteNonQuery();

				// CREATE TABLE CARD
				var createCardTableCmd = new NpgsqlCommand(
					"CREATE TABLE IF NOT EXISTS cards (" +
					"id SERIAL PRIMARY KEY, " +
					"guid UUID UNIQUE, " +
					"name VARCHAR(50), " +
					"type VARCHAR(50), " +
					"element VARCHAR(50), " +
					"damage INT)", conn);
				createCardTableCmd.ExecuteNonQuery();

				// CREATE TABLE CARD
				var createTradingDealsTableCmd = new NpgsqlCommand(
					"CREATE TABLE IF NOT EXISTS tradingdeals (" +
					"id UUID PRIMARY KEY, " +
					"card_guid UUID UNIQUE, " +
					"type VARCHAR(50), " +
					"owner_id INT REFERENCES players(id), " +
					"mindamage INT)", conn);
				createTradingDealsTableCmd.ExecuteNonQuery();



			}
		}
    }
}
