using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Models
{
	public class PlayerStats
	{
		public string? Name { get; set; }
		public int Elo { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public double WinRate { get; set; }

		public void CalculateWinRate()
		{
			if (Wins == 0 && Losses == 0)
				WinRate = 0;
			else if (Wins == 0)
				WinRate = 0;
			else if (Losses == 0)
				WinRate = 1;
			else
				WinRate = Math.Round((double)Wins / (Wins + Losses), 2);
		}	

	}
}
