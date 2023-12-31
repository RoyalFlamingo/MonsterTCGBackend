using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTCG.Business;

namespace MonsterTCG.Business.Models
{
	public class GamePlayer
	{
		public Player Player { get; set; }
		public List<Card> Deck { get; set; }
		public int Points { get; set; }

		public GamePlayer(Player player, List<Card> deck)
		{
			Player = player;
			Deck = deck;
			Points = 0;
		}

	}
}
