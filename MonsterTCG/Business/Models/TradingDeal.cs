using MonsterTCG.Business.Enums;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Models
{
	public class TradingDeal
	{
		public string? Id { get; set; }
		public string? CardToTrade { get; set; }
		public CardType Type { get; set; }
		public int MinimumDamage { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public int OwnerId { get; set; }

	}
}
