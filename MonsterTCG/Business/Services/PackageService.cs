using MonsterTCG.Business.Database;
using MonsterTCG.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Services
{
	class PackageService
	{
		private readonly PlayerRepository _playerRepository;
		private readonly CardRepository _cardRepository;

		public PackageService()
		{
			_playerRepository = new PlayerRepository();
			_cardRepository = new CardRepository();
		}


		/// <summary>
		/// Creates a new package from an array of cards. Only the "admin" user may do this
		/// </summary>
		/// <returns>true if the package was created</returns>
		public async Task<bool> CreatePackage(IEnumerable<Card> cards)
		{
			if(cards.Count() == 0)
				return false;

			if (!await _cardRepository.CreatePackage(cards))
				return false;

			return true;
		}

		/// <summary>
		/// Creates a new package from an array of cards. Only the "admin" user may do this
		/// </summary>
		/// <returns>true if the package was created</returns>
		public async Task<bool> BuyPackage(string token)
		{
			var player = await _playerRepository.GetPlayer(token);

			if(player == null)
				throw new UnauthorizedAccessException();

			if (player.Coins < 5)
				throw new InsufficientCoinsException();
			
			if (!await _cardRepository.BuyPackage(player))
				return false;

			//remove 5 coins after buying the package
			player.Coins -= 5;

			if (!await _playerRepository.UpdatePlayer(player))
				return false;

			return true;
		}
	}
}
