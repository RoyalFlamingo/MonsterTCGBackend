using MonsterTCG.Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Models
{
	class Card
	{
		[Newtonsoft.Json.JsonIgnore]
		public int Id { get; set; }
		public string? Guid { get; set; }
		public string? Name { get; set; }
		public CardType Type { get; set; }
		public CardElement Element { get; set; }
		public int Damage { get; set; }

	}
}
