using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTCG.Business.Migrations
{
	internal interface IMigration
	{

		int Version { get; }
		public void Up();

		//migrations should also have a down method, but too lazy
	}
}
