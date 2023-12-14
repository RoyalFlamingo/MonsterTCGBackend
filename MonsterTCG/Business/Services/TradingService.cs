using MonsterTCG.Business.Database;
using MonsterTCG.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Services
{
	class TradingService
	{
		private readonly PlayerRepository _playerRepository;
		private readonly CardRepository _cardRepository;

		public TradingService()
		{
			_playerRepository = new PlayerRepository();
			_cardRepository = new CardRepository();
		}


		/// <summary>
		/// Creates a new trading deal. You can only create a deal for a card you own.
		/// </summary>
		/// <returns>true if the deal was created</returns>
		public async Task<bool> CreateTradingDeal(TradingDeal deal, string token)
		{
			if (token == "")
				throw new UnauthorizedAccessException();

			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			deal.OwnerId = tokenPlayer.Id;

			return await _playerRepository.CreateTradingDeal(deal);
		}

		/// <summary>
		/// Retrieves the currently available trading deals.
		/// </summary>
		/// <returns>found trading deal</returns>
		public async Task<List<TradingDeal>> FetchTradingDeals(string token)
		{
			if (token == "")
				throw new UnauthorizedAccessException();

			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			return await _playerRepository.GetTradingDeals();
		}


	}
}
