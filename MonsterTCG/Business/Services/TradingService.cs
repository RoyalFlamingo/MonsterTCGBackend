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

		/// <summary>
		/// Deletes an existing trading deal. It is only allowed if the user owns the associated card.
		/// </summary>
		/// <returns>true if deal was deleted</returns>
		public async Task<bool> DeleteTradingDeal(string token, string tradingdealid)
		{
			if (token == "")
				throw new UnauthorizedAccessException();

			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			var allDeals = await _playerRepository.GetTradingDeals();
			allDeals = allDeals.Where(x => x.OwnerId == tokenPlayer.Id && x.Id == tradingdealid).ToList();

			if(allDeals.Count == 0)
				return false;

			return await _playerRepository.DeleteTradingDeal(tradingdealid);
		}

		/// <summary>
		/// Carry out a trade for the deal with the provided card. Trading with self is not allowed.
		/// </summary>
		/// <returns>true if deal was executed</returns>
		public async Task<bool> Trade(string token, string tradingdealid, string cardId)
		{
			if (token == "")
				throw new UnauthorizedAccessException();

			//tokenplayer is the player that wants to conclude the trade
			var tokenPlayer = await _playerRepository.GetPlayer(token);

			if (tokenPlayer == null)
				throw new UnauthorizedAccessException();

			//find the deal
			var allDeals = await _playerRepository.GetTradingDeals();
			allDeals = allDeals.Where(x => x.OwnerId == tokenPlayer.Id && x.Id == tradingdealid).ToList();

			if (allDeals.Count == 0)
				throw new DealNotFoundException();

			TradingDeal deal = allDeals[0];

			if (deal.OwnerId == tokenPlayer.Id)
				return false;

			//find the cards owned by the player that wants to conclude the trade
			var tokenCardStack = await _cardRepository.GetStack(tokenPlayer.Id);
			tokenCardStack = tokenCardStack.Where(x => x.Guid == cardId).ToList();

			if (tokenCardStack.Count == 0)
				return false;

			Card tokenCard = tokenCardStack[0];

			//find the cards of the player that created the deal
			var ogCardStack = await _cardRepository.GetStack(deal.OwnerId);
			ogCardStack = ogCardStack.Where(x => x.Guid == cardId).ToList();

			if (ogCardStack.Count == 0)
				return false;

			Card ogCard = ogCardStack[0];

			//check for minimum damage and type
			if (tokenCard.Damage < deal.MinimumDamage || tokenCard.Type != deal.Type)
				return false;


			return await _playerRepository.Trade(ogCard, tokenCard, deal.OwnerId, tokenPlayer.Id, deal.Id);
		}


	}
}
