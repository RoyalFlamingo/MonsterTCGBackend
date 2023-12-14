using MonsterTCG.Business.Models;

/// <summary>
/// Singleton class used to queue players for battle and handle the battle logic
/// </summary>
public class BattleQueue
{
	private static readonly Queue<GamePlayer> _waitingPlayers = new Queue<GamePlayer>();
	private static readonly object _lock = new object();
	private static BattleQueue _instance;

	private BattleQueue() { }

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

				BattleLogic(player1, player2);

				return true;
			}

			//lock is removed when there are less than 2 players in the queue
			return false;
		}
	}

	private void BattleLogic(GamePlayer player1, GamePlayer player2)
	{
		Console.WriteLine("Battle between " + player1.Player.Name + " and " + player2.Player.Name + " started!");
	}
}