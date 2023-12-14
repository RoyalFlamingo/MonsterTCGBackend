using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Models
{
	class PlayerStats
	{
		public string? Name { get; set; }
		public int Elo { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }

	}
}
