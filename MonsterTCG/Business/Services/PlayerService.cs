using MonsterTCG.Business.Database;
using MonsterTCG.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Services
{
	class PlayerService
	{
		private readonly PlayerRepository _playerRepository;

		public PlayerService()
		{
			_playerRepository = new PlayerRepository();
		}

		public string GenerateToken(string playername)
		{
			return $"Bearer {playername}-mtcgToken";
		}

		/// <summary>
		/// Inserts a new player entry into the database
		/// </summary>
		/// <returns>true if the player was created</returns>
		public async Task<bool> CreatePlayer(string playername, string password)
		{
			var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

			if (await _playerRepository.CreatePlayer(playername, hashedPassword))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Update the player entry in the database
		/// </summary>
		/// <returns>true if the player was updated</returns>
		public async Task<bool> UpdatePlayer(Player player, string token)
		{
			if (player.AccountName == null)
				throw new UnauthorizedAccessException();

			if (token != "Bearer admin-mtcgToken")
			{
				var tokenPlayer = await _playerRepository.GetPlayer(token);

				if (tokenPlayer.Token != token)
					throw new UnauthorizedAccessException();

				if (tokenPlayer.Token != token)
					throw new UnauthorizedAccessException();
			}

			var foundPlayer = await _playerRepository.GetPlayerWithName(player.AccountName);

			if (foundPlayer == null)
				return false;

			var toUpdate = new Player()
			{
				Id = foundPlayer.Id,
				AccountName = foundPlayer.AccountName,
				Password = foundPlayer.Password,
				Name = player.Name,
				Coins = foundPlayer.Coins,
				Elo = foundPlayer.Elo,
				Wins = foundPlayer.Wins,
				Losses = foundPlayer.Losses,
				Bio = player.Bio,
				Image = player.Image,
				Token = foundPlayer.Token
			};

			if (!await _playerRepository.UpdatePlayer(toUpdate))
				return false;

			return true;
		}

		/// <summary>
		/// Gets the player profile from the database
		/// </summary>
		/// <returns>player profile or null if not found</returns>
		public async Task<Player> GetProfile(string playername, string token)
		{
			if (playername == "")
				throw new UnauthorizedAccessException();

			if (token != "Bearer admin-mtcgToken")
			{
				var tokenPlayer = await _playerRepository.GetPlayer(token);

				if (tokenPlayer.Token != token)
					throw new UnauthorizedAccessException();

				if (tokenPlayer.Token != token)
					throw new UnauthorizedAccessException();
			}

			var foundPlayer = await _playerRepository.GetPlayerWithName(playername);

			if (foundPlayer == null)
				return null;

			return foundPlayer;
		}

		/// <summary>
		/// Checks for username/password combination and sets the player token
		/// </summary>
		/// <returns>true if login was successful</returns>
		public async Task<bool> PlayerLogin(string playername, string password)
		{
			var player = await _playerRepository.GetPlayerWithName(playername);

			if (player == null)
				return false;

			var hashedPassword = BCrypt.Net.BCrypt.Verify(password, player.Password);

			if (!hashedPassword)
				return false;

			player.Token = GenerateToken(player.AccountName);

			if (!await _playerRepository.UpdatePlayer(player))
				return false;

			return true;
		}

		/// <summary>
		/// Gets the player stats from the database
		/// </summary>
		/// <returns>stats or null if not found</returns>
		public async Task<PlayerStats> GetStats(string token)
		{
			if (token == "")
				throw new UnauthorizedAccessException();

			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			return new PlayerStats()
			{
				Name = tokenPlayer.Name,
				Elo = tokenPlayer.Elo,
				Wins = tokenPlayer.Wins,
				Losses = tokenPlayer.Losses
			};
		}

		/// <summary>
		/// Gets the player stats from the database
		/// </summary>
		/// <returns>stats or null if not found</returns>
		public async Task<List<PlayerStats>> GetScoreboard()
		{
			return await _playerRepository.GetScoreboard();
		}
	}
}
