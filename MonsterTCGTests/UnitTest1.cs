using NUnit.Framework;
using MonsterTCG.Business;
using MonsterTCG.Business.Database;
using MonsterTCG.Business.Models;
using MonsterTCG.Business.Services;
using MonsterTCG.Business.Enums;
using MonsterTCG.Config;
using System;
using MonsterTCG.Http;
using System.Net.Sockets;
using MonsterTCG.Controllers;
using System.Net;

namespace MonsterTCGTests
{
	public class Tests
	{
		private static readonly Random _random = new Random();
		private PlayerService _playerService;


		[SetUp]
		public void Setup()
		{
			ConfigurationManager.LoadConfiguration();
			_playerService = new PlayerService();
		}

		public static Card CreateRandomCard()
		{
			return new Card
			{
				Id = _random.Next(1, 1000),
				Guid = Guid.NewGuid().ToString(),
				Name = GetRandomCardName(),
				Type = CardType.Monster,
				Element = CardElement.Normal,
				Damage = _random.Next(1, 100),
				CritChance = _random.NextDouble()
			};
		}

		private static string GetRandomCardName()
		{
			var names = new[] { "Dragon", "Knight", "Elf", "Goblin", "Wizard", "Ork", "Kraken", "FireElf" };
			return names[_random.Next(names.Length)];
		}

		//1
		[Test]
		public void ChooseRandomCard_ShouldReturnCardFromDeck()
		{
			// Arrange
			var deck = new List<Card> { new Card(), new Card(), new Card() };
			var battleLogic = new BattleLogic();

			// Act
			var card = battleLogic.ChooseRandomCard(deck, new Random());

			// Assert
			Assert.Contains(card, deck);
		}

		//2
		[TestCase(CardElement.Water, CardElement.Fire, 10, 20)]
		[TestCase(CardElement.Fire, CardElement.Normal, 10, 20)]
		public void AdjustDamageForElement_ShouldAdjustDamage(CardElement attacker, CardElement defender, double damage, double expectedDamage)
		{
			// Arrange
			var battleLogic = new BattleLogic();

			// Act
			var adjustedDamage = battleLogic.AdjustDamageForElement(attacker, defender, damage);

			// Assert
			Assert.AreEqual(expectedDamage, adjustedDamage);
		}

		//3
		[Test]
		public void AdjustDamageForCrit_ShouldDoubleDamageOnCrit()
		{
			// Arrange
			var card = new Card { CritChance = 1.0 }; // 100% Crit Chance
			var battleLogic = new BattleLogic();
			var log = new List<string>();

			// Act
			var adjustedDamage = battleLogic.AdjustDamageForCrit(10, card, log);

			// Assert
			Assert.AreEqual(20, adjustedDamage);
		}

		//4
		[Test]
		public void AdjustDamageForSpecialRule_GoblinVsDragon()
		{
			var goblinCard = new Card { Name = "Goblin" };
			var dragonCard = new Card { Name = "Dragon" };
			var log = new List<string>();

			var battleLogic = new BattleLogic();
			var damage = battleLogic.AdjustDamageForSpecialRule(goblinCard, dragonCard, log);

			Assert.AreEqual(0, damage);
		}

		//5
		[Test]
		public void AdjustDamageForSpecialRule_OrkVsWizzard()
		{
			var orkCard = new Card { Name = "Ork" };
			var wizzardCard = new Card { Name = "Wizzard" };
			var log = new List<string>();

			var battleLogic = new BattleLogic();
			var damage = battleLogic.AdjustDamageForSpecialRule(orkCard, wizzardCard, log);

			Assert.AreEqual(0, damage);
		}

		//6
		[Test]
		public void AdjustDamageForSpecialRule_WaterSpellVsKnight()
		{
			var waterSpellCard = new Card { Type = CardType.Spell, Element = CardElement.Water };
			var knightCard = new Card { Name = "Knight" };
			var log = new List<string>();

			var battleLogic = new BattleLogic();
			var damage = battleLogic.AdjustDamageForSpecialRule(waterSpellCard, knightCard, log);

			Assert.AreEqual(999, damage); // Instant kill
		}

		//7
		[Test]
		public void AdjustDamageForSpecialRule_SpellVsKraken()
		{
			var spellCard = new Card { Type = CardType.Spell };
			var krakenCard = new Card { Name = "Kraken" };
			var log = new List<string>();

			var battleLogic = new BattleLogic();
			var damage = battleLogic.AdjustDamageForSpecialRule(spellCard, krakenCard, log);

			Assert.AreEqual(0, damage); // Immune
		}

		//8
		[Test]
		public void AdjustDamageForSpecialRule_DragonVsFireElf()
		{
			var dragonCard = new Card { Name = "Dragon" };
			var fireElfCard = new Card { Name = "FireElf" };
			var log = new List<string>();

			var battleLogic = new BattleLogic();
			var damage = battleLogic.AdjustDamageForSpecialRule(dragonCard, fireElfCard, log);

			Assert.AreEqual(0, damage); // Evasion
		}

		//9
		[Test]
		public async Task Battle_ShouldLogBattleStart()
		{
			// Arrange
			var player1 = new GamePlayer(new Player(), new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var player2 = new GamePlayer(new Player(), new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var battleLogic = new BattleLogic();

			// Act
			var log = await battleLogic.Battle(player1, player2);

			// Assert
			StringAssert.Contains("Battle between  and  started!", log.First());
		}

		//10
		[Test]
		public async Task Battle_ShouldLogBattleEnd()
		{
			// Arrange
			var player1 = new GamePlayer(new Player(), new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var player2 = new GamePlayer(new Player(), new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var battleLogic = new BattleLogic();

			// Act
			var log = await battleLogic.Battle(player1, player2);

			// Assert
			StringAssert.Contains("wins the battle", log.Last());
		}

		//11
		[Test]
		public async Task CalculateAndWriteElo_ShouldChangeElo()
		{
			// Arrange
			var winner = new GamePlayer(new Player() { Elo = 1000 }, new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var loser = new GamePlayer(new Player() { Elo = 1000 }, new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var battleLogic = new BattleLogic();

			// Act
			var eloChange = await battleLogic.CalculateAndWriteElo(winner, loser);

			// Assert
			Assert.AreNotEqual(0, eloChange);
		}

		//12
		[Test]
		public void WriteLogFile_ShouldCreateLogFile()
		{
			// Arrange
			var log = new List<string> { "Test log entry" };
			var battleLogic = new BattleLogic();
			string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

			// Sicherstellen, dass das Verzeichnis existiert
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}

			// Act
			battleLogic.WriteLogFile(log);

			// Assert
			var logFiles = Directory.GetFiles(logDirectory);
			Assert.IsTrue(logFiles.Length > 0, "No log file created.");
		}

		//13
		[Test]
		public void GenerateToken_WithFakeTokensEnabled_ShouldReturnFakeToken()
		{
			// Arrange
			ConfigurationManager.UseFakeTokens = true; // Enable fake tokens

			// Act
			var token = _playerService.GenerateToken("testPlayer");

			// Assert
			Assert.IsTrue(token.StartsWith("Bearer testPlayer-mtcgToken"));
		}

		//14
		[Test]
		public void GenerateToken_WithFakeTokensDisabled_ShouldReturnRandomToken()
		{
			// Arrange
			ConfigurationManager.UseFakeTokens = false; // Disable fake tokens

			// Act
			var token = _playerService.GenerateToken("testPlayer");

			// Assert
			Assert.IsFalse(token.StartsWith("Bearer testPlayer-mtcgToken"));
			Assert.IsNotEmpty(token);
		}

		//15
		[Test]
		public void GetProfile_EmptyPlayername_ShouldThrowUnauthorizedAccessException()
		{
			// Arrange
			var playerService = new PlayerService();

			// Act & Assert
			Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
				await playerService.GetProfile("", "someToken"));
		}

		//16
		[Test]
		public void RequestHandler_Constructor_ShouldThrowArgumentNullExceptionForNullArguments()
		{
			TcpClient tcpClient = null;
			Server httpServer = null;

			Assert.Throws<ArgumentNullException>(() => new RequestHandler(tcpClient, httpServer));
		}

		//17
		[Test]
		public void ProcessLine_ShouldCorrectlyParseRequestLine()
		{
			// Arrange
			var tcpClient = new TcpClient();
			var httpServer = new Server(8080);
			var requestHandler = new RequestHandler(tcpClient, httpServer);

			// Act
			requestHandler.ProcessLine("GET /home HTTP/1.1");

			// Assert
			Assert.AreEqual("GET", requestHandler.req.Method);
			Assert.AreEqual("/home", requestHandler.req.Path);
			Assert.AreEqual("HTTP/1.1", requestHandler.req.Version);
		}

		//18
		[Test]
		public void GetInstance_ShouldReturnSameInstance()
		{
			var instance1 = BattleQueue.GetInstance();
			var instance2 = BattleQueue.GetInstance();

			Assert.AreSame(instance1, instance2);
		}

		//19
		[Test]
		public async Task EnterBattle_WithTwoPlayers_ShouldStartBattle()
		{
			// Arrange
			var player1 = new GamePlayer(new Player(), new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var player2 = new GamePlayer(new Player(), new List<Card> { CreateRandomCard(), CreateRandomCard(), CreateRandomCard(), CreateRandomCard() });
			var queue = BattleQueue.GetInstance();

			// Act
			var task1 = queue.EnterBattle(player1);
			var task2 = queue.EnterBattle(player2);
			await Task.WhenAll(task1, task2);

			// Assert
			Assert.IsNotNull(task2.Result);
		}

		//20
		[Test]
		public async Task GetStack_WithoutAuthorizationToken_ShouldReturnBadRequestResponse()
		{
			// Arrange
			var cardController = new CardController();
			var request = new Request
			{
				Headers = new Dictionary<string, string>()
			};

			// Act
			var response = await cardController.GetStack(request);

			// Assert
			Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
			Assert.AreEqual("Access token is missing or invalid", response.Content);
		}

	}
}