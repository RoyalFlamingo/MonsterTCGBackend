using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Models
{
	public class CardCollection
	{
		public int Id { get; set; }
		public string? CardGuid { get; set; }
		public int OwnerId { get; set; }
	}
}
