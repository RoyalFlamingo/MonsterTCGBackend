using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Models
{
	public class Player
	{
		public int Id { get; set; }
		public string? AccountName { get; set; }
		public string? Password { get; set; }
		public string? Name { get; set; }
		public int Coins { get; set; }
		public int Elo { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public string? Bio { get; set; }
		public string? Image { get; set; }
		public string? Token { get; set; }


	}
}
