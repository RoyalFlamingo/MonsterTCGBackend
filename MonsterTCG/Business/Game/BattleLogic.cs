using MonsterTCG.Business.Database;
using MonsterTCG.Business.Enums;
using MonsterTCG.Business.Models;
using MonsterTCG.Config;
using System.Text;

public class BattleLogic
{
	private readonly PlayerRepository _playerRepository;
	public BattleLogic()
	{
		_playerRepository = new PlayerRepository();
	}

	/// <summary>
	/// Concludes the battle between two players
	/// </summary>
	/// <returns>A list of battle log entries</returns>
	public async Task<List<string>> Battle(GamePlayer player1, GamePlayer player2)
	{
		List<string> battleLog = new List<string>();

		Console.WriteLine("Battle between " + player1.Player.Name + " and " + player2.Player.Name + " started!");

		const int maxRounds = 100;

		var seed = Guid.NewGuid().GetHashCode(); //get a random seed
		Random random = new Random(seed); //sets the random seed, could be set manually for testing

		for (int round = 0; round < maxRounds; round++)
		{
			if (!player1.Deck.Any() || !player2.Deck.Any())
			{
				break; //no cards left
			}

			//choose cards for current round
			var card1 = ChooseRandomCard(player1.Deck, random);
			var card2 = ChooseRandomCard(player2.Deck, random);

			battleLog.Add($"Round {round + 1}: {card1.Name} (Type: {card1.Type}, Element: {card1.Element}, Damage: {card1.Damage} VS {card2.Name} (Type: {card2.Type}, Element: {card2.Element}, Damage: {card2.Damage}");

			var damage1 = AdjustDamageForSpecialRule(card1, card2, battleLog);
			var damage2 = AdjustDamageForSpecialRule(card2, card1, battleLog);

			if (card1.Type == CardType.Spell || card2.Type == CardType.Spell)
			{
				damage1 = AdjustDamageForElement(card1.Element, card2.Element, damage1);
				damage2 = AdjustDamageForElement(card2.Element, card1.Element, damage2);
			}

			if (damage1 > damage2)
			{
				// player 1 wins the round

				//await _playerRepository.ChangeCardOwnership(card2, player1.Player.Id); // old card change ownership
				player2.Deck.Remove(card2);
				player1.Deck.Add(card2);
				player1.Points++;
				battleLog.Add($"Player 1 wins round {round + 1} with {damage1} damage VS {damage2} damage. Player 1 won the card {card2.Name}!");
			}
			else if (damage2 > damage1)
			{
				// player 2 wins the round

				//await _playerRepository.ChangeCardOwnership(card1, player2.Player.Id); // old card change ownership
				player1.Deck.Remove(card1);
				player2.Deck.Add(card1);
				player2.Points++;
				battleLog.Add($"Player 2 wins round {round + 1} with {damage2} damage VS {damage1} damage. Player 2 won the card {card1.Name}!");
			}
			else
			{
				// draw
				battleLog.Add($"Both cards have {damage1} damage. Draw!");

				//if both players have only 1 card left and both draw, the game ends in a draw
				if (player1.Deck.Count == 1 && player2.Deck.Count == 1)
				{
					break;
				}
			}
		}

		if (player1.Points > player2.Points)
		{
			var elo = await CalculateAndWriteElo(player1, player2);
			battleLog.Add($"Player 1 wins the battle with {player1.Points} points VS {player2.Points} points! Player 1 gains {elo} elo.");
		}
		else if (player2.Points > player1.Points)
		{
			var elo = await CalculateAndWriteElo(player2, player1);
			battleLog.Add($"Player 2 wins the battle with {player2.Points} points VS {player1.Points} points! Player 2 gains {elo} elo.");
		}
		else
		{
			battleLog.Add($"The battle ended in a draw with {player1.Points} points each!");
		}

		//print log to server console
		foreach (var logEntry in battleLog)
		{
			Console.WriteLine(logEntry);
		}

		//save log to file
		WriteLogFile(battleLog);

		return battleLog;
	}

	private Card ChooseRandomCard(List<Card> deck, Random random)
	{
		int index = random.Next(deck.Count);
		return deck[index];
	}
	private double AdjustDamageForElement(CardElement attackerElement, CardElement defenderElement, double damage)
	{
		if (attackerElement == defenderElement)
		{
			return damage;
		}

		switch (attackerElement)
		{
			case CardElement.Water:
				if (defenderElement == CardElement.Fire)
					return damage * 2;
				break;
			case CardElement.Fire:
				if (defenderElement == CardElement.Normal)
					return damage * 2;
				break;
			case CardElement.Normal:
				if (defenderElement == CardElement.Water)
					return damage * 2;
				break;
			case CardElement.Wind:
				if (defenderElement == CardElement.Thunder)
					return damage * 2;
				break;
			case CardElement.Thunder:
				if (defenderElement == CardElement.Wind)
					return damage * 2;
				break;
		}

		return damage * 0.5; //in all other cases, the attacking element is weaker
	}

	private double AdjustDamageForSpecialRule(Card dealer, Card taker, List<string> log)
	{
		if (dealer.Name == "Goblin" && taker.Name == "Dragon")
		{
			log.Add($"Goblins are too afraid of Dragons to attack");
			return 0;
		}
		if (dealer.Name == "Ork" && taker.Name == "Wizzard")
		{
			log.Add($"Wizzard can control Orks so they are not able to damage them.");
			return 0;
		}
		if (dealer.Type == CardType.Spell && dealer.Element == CardElement.Water && taker.Name == "Knight")
		{
			log.Add($"The armor of Knights is so heavy that WaterSpells make them drown them instantly.");
			return 999; // insta kill
		}
		if (dealer.Type == CardType.Spell && taker.Name == "Kraken")
		{
			log.Add($"The Kraken is immune against spells.");
			return 0;
		}
		if (dealer.Name == "Dragon" && taker.Name == "FireElf")
		{
			log.Add($"The FireElves know Dragons since they were little and can evade their attacks.");
			return 0;
		}

		return dealer.Damage;
	}

	private void WriteLogFile(List<string> log)
	{
		string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

		if (!Directory.Exists(logDirectory)) //check if log folder exists, otherwise create it
		{
			Directory.CreateDirectory(logDirectory);
		}

		var random = new Random();
		string fileName = $"BattleLog_{DateTime.Now:yyyyMMddHHmmss}_{random.Next(1000, 9999)}.txt"; //write unique file name for log file
		string filePath = Path.Combine(logDirectory, fileName);

		File.WriteAllLines(filePath, log);
	}

	private async Task<int> CalculateAndWriteElo(GamePlayer winner, GamePlayer loser)
	{
		var eloChange = ConfigurationManager.EloKFactor * ((1 - (1 / (1 + Math.Pow(10, (loser.Player.Elo - winner.Player.Elo) / 400.0d)))));
		var roundedEloChange = Convert.ToInt32(eloChange);

		winner.Player.Elo += roundedEloChange;
		loser.Player.Elo = Math.Max(0, (loser.Player.Elo - roundedEloChange));

		await _playerRepository.UpdatePlayer(winner.Player);
		await _playerRepository.UpdatePlayer(loser.Player);

		return roundedEloChange;
	}
}