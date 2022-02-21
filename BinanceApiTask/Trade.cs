using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceApiTask
{
	internal class Trade
	{
		//public string Type { get; set; }
		//public string Time { get; set; }
		[JsonProperty("s")]
		public string Symbol { get; set; }
		//public int AggregateTradeID { get; set; }
		[JsonProperty("p")]
		public string Price { get; set; }
		[JsonProperty("q")]
		public double Quantity { get; set; }
		//public int FirstTradeID { get; set; }
		//public int LastTradeID { get; set; }
		//public int TradeTime { get; set; }
		[JsonProperty("m")]
		public bool IsBuyer { get; set; }


		//  "e": "aggTrade",  // Event type
		//  "E": 123456789,   // Event time
		//  "s": "BTCUSDT",    // Symbol
		//  "a": 5933014,     // Aggregate trade ID
		//  "p": "0.001",     // Price
		//  "q": "100",       // Quantity
		//  "f": 100,         // First trade ID
		//  "l": 105,         // Last trade ID
		//  "T": 123456785,   // Trade time
		//  "m": true,        // Is the buyer the market maker?

	}
}
