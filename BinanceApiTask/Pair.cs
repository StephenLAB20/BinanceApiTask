using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceApiTask
{
	internal class Pair
	{
		public string Name { get; set; }
		public Dictionary<string, QuoteValue> pairValues { get; set; }
	}
}
