using MonsterTCG.Business.Database;
using MonsterTCG.Business.Enums;
using MonsterTCG.Business.Models;
using System.Text;

/// <summary>
/// Singleton class used to queue players for battle and handle the battle logic
/// </summary>
public class BattleQueue
{
	private static readonly Queue<GamePlayer> _waitingPlayers = new Queue<GamePlayer>();
	private static readonly object _lock = new object();
	private static BattleQueue _instance;
	private static readonly PlayerRepository _playerRepository = new PlayerRepository();

	public static BattleQueue GetInstance() //singleton
	{
		if (_instance == null)
		{
			_instance = new BattleQueue();
		}
		return _instance;
	}

	public bool EnterBattle(GamePlayer player)
	{
		lock (_lock) //lock to only allow one thread to access the queue at a time
		{
			_waitingPlayers.Enqueue(player);

			if (_waitingPlayers.Count >= 2)
			{
				GamePlayer player1 = _waitingPlayers.Dequeue();
				GamePlayer player2 = _waitingPlayers.Dequeue();

				Task.Run(() => BattleLogic(player1, player2)); //run battle in a new thread

				return true;
			}

			//lock is removed when there are less than 2 players in the queue
			return false;
		}
	}

	/// <summary>
	/// Concludes the battle between two players
	/// </summary>
	/// <returns>A list of battle log entries</returns>
	private async Task BattleLogic(GamePlayer player1, GamePlayer player2)
	{
		List<string> battleLog = new List<string>();

		Console.WriteLine("Battle between " + player1.Player.Name + " and " + player2.Player.Name + " started!");

		const int maxRounds = 100;
		Random random = new Random();

		for (int round = 0; round < maxRounds; round++)
		{
			if (!player1.Deck.Any() || !player2.Deck.Any())
			{
				break; //no cards left
			}

			//choose cards for current round
			var card1 = ChooseRandomCard(player1.Deck, random);
			var card2 = ChooseRandomCard(player2.Deck, random);

			double damage1 = CalculateDamage(card1);
			double damage2 = CalculateDamage(card2);

			battleLog.Add($"Round {round + 1}: {card1.Name} (Type: {card1.Type}, Element: {card1.Element}, Damage: {card1.Damage} VS {card2.Name} (Type: {card2.Type}, Element: {card2.Element}, Damage: {card2.Damage}");

			damage1 = AdjustDamageForSpecialRule(card1, card2, battleLog);
			damage2 = AdjustDamageForSpecialRule(card2, card1, battleLog);

			if (card1.Type == CardType.Spell || card2.Type == CardType.Spell)
			{
				damage1 = AdjustDamageForElement(card1.Element, card2.Element, damage1);
				damage2 = AdjustDamageForElement(card2.Element, card1.Element, damage2);
			}

			if (damage1 > damage2)
			{
				// player 1 wins the round
				await _playerRepository.ChangeCardOwnership(card2, player1.Player.Id);
				battleLog.Add($"Player 1 wins round {round+1} with {damage1} damage VS {damage2} damage.");
			}
			else if (damage2 > damage1)
			{
				// player 2 wins the round
				await _playerRepository.ChangeCardOwnership(card1, player2.Player.Id);
				battleLog.Add($"Player 2 wins round {round + 1} with {damage2} damage VS {damage1} damage.");
			}
			else
			{
				// draw
				battleLog.Add($"Both cards have {damage1} damage. Draw!");
			}

			WriteLog(battleLog);

			return;

		}

		foreach (var logEntry in battleLog)
		{
			Console.WriteLine(logEntry);
		}
	}

	private Card ChooseRandomCard(List<Card> deck, Random random)
	{
		int index = random.Next(deck.Count);
		return deck[index];
	}

	private double CalculateDamage(Card card)
	{
		return card.Damage;
	}

	private double AdjustDamageForElement(CardElement attackerElement, CardElement defenderElement, double damage)
	{
		return damage;
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

	private void WriteLog(List<string> log)
	{
		string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
		var random = new Random();
		string fileName = $"BattleLog_{DateTime.Now:yyyyMMddHHmmss}_{random.Next(1000, 9999)}.txt";
		string filePath = Path.Combine(logDirectory, fileName);

		File.WriteAllLines(filePath, log);
	}
}