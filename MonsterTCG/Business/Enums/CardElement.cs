using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CardElement
	{
		Normal,
		Fire,
		Water
	}
}
