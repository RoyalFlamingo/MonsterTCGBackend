using MonsterTCG.Business.Database;
using MonsterTCG.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Services
{
	class CardService
	{
		private readonly PlayerRepository _playerRepository;
		private readonly CardRepository _cardRepository;

		public CardService()
		{
			_playerRepository = new PlayerRepository();
			_cardRepository = new CardRepository();
		}

		/// <summary>
		/// Gets the player stack from the database
		/// </summary>
		/// <returns>player stack</returns>
		public async Task<List<Card>> GetStack(string token)
		{
			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			return await _cardRepository.GetStack(tokenPlayer.Id);
		}

		/// <summary>
		/// Gets the player deck from the database
		/// </summary>
		/// <returns>player deck</returns>
		public async Task<List<Card>> GetDeck(string token)
		{
			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			return await _cardRepository.GetDeck(tokenPlayer.Id);
		}

		/// <summary>
		/// Sets the player deck in the database
		/// </summary>
		/// <returns>true is player deck was set</returns>
		public async Task<bool> SetDeck(IEnumerable<string> cardIds, string token)
		{
			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			return await _cardRepository.SetDeck(cardIds, tokenPlayer.Id);
		}

	}
}
