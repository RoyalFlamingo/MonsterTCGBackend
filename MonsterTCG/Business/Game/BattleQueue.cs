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

	public async Task<List<string>> EnterBattle(GamePlayer player)
	{
		bool shouldStartBattle = false;
		GamePlayer player1 = null;
		GamePlayer player2 = null;

		lock (_lock)
		{
			_waitingPlayers.Enqueue(player);

			if (_waitingPlayers.Count >= 2)
			{
				player1 = _waitingPlayers.Dequeue();
				player2 = _waitingPlayers.Dequeue();
				shouldStartBattle = true;
			}
		}

		if (shouldStartBattle)
		{
			var battleLogic = new BattleLogic();
			var log = await battleLogic.Battle(player1, player2); // Run battle outside lock
			return log;
		}

		return null;
	}
}