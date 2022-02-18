using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;



namespace BinanceApiTask
{
	internal class Program
	{
		public static void Main()
		{
			string input = "bnb/usdt, btc/usdt eth/usdt";
			string[] inputArr = input.Replace("/", "").Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			Dictionary<string, List<QuoteValue>> trades = new Dictionary<string, List<QuoteValue>>();
			int maxTradesCount = 10000;


			for (int i = 0; i < inputArr.Length; i++)
			{
				var j = i;
				string tempInput = inputArr[j];
				var thread = new Thread(() =>
				{
					string url = "wss://fstream.binance.com/ws/" + tempInput + "@aggTrade";

					var ws = new WebSocket(url);
					ws.OnMessage += (sender, e) =>
					{
						QuoteValue trade = new QuoteValue();
						try
						{
							var parsedObject = JObject.Parse(e.Data);
							var jsonKeys = parsedObject.ToString();

							trade = JsonConvert.DeserializeObject<QuoteValue>(jsonKeys);
							trades = AddToPairsDictionary(trades, trade);

							PrintData(trades);

						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
					};
					ws.Connect();
				});
				thread.Start();
			}

			Thread threadCleaner = new Thread(() =>
			{
				while (trades.Count > 0)
				{
					trades = ClearDictionary(trades, maxTradesCount);
					Thread.Sleep(60000);
				}
			}
			);
			threadCleaner.Start();

			Console.ReadLine();

		}

		private static Dictionary<string, List<QuoteValue>> ClearDictionary(Dictionary<string, List<QuoteValue>> trades, int maxTradesCount)
		{
			foreach (var trade in trades)
			{
				if (trade.Value.Count > maxTradesCount)
				{
					trade.Value.RemoveRange(0, trade.Value.Count - maxTradesCount);
				}
			}
			return trades;
		}

		private static Dictionary<string, List<QuoteValue>> AddToPairsDictionary(Dictionary<string, List<QuoteValue>> trades, QuoteValue trade)
		{
			string symbol = trade.Symbol;

			if (!trades.ContainsKey(symbol))
			{
				trades[symbol] = new List<QuoteValue>();
				//trades.Add(symbol, new List<QuoteValue>());
			}
			//trades[symbol].Add(trade);
			trades[symbol].Add(trade);

			return trades;
		}

		private static void PrintData(Dictionary<string, List<QuoteValue>> trades)
		{
			// TODO print updated trade separately

			//Console.CursorVisible = false;
			Console.SetCursorPosition(0, 0);

			foreach (var trade in trades)
			{
				string symbol = trade.Key;
				string price = trade.Value.Last().Price;
				bool isBuyer = trade.Value.Last().IsBuyer;

				if (isBuyer)
				{
					Console.BackgroundColor = ConsoleColor.Green;
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("{0,-10}{1,-10}", symbol, price);
					//Console.WriteLine("count {0} {1}", trade.Key, trade.Value.Count);
				}
				else
				{
					Console.BackgroundColor = ConsoleColor.Red;
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("{0,-10}{1,-10}", symbol, price);
					//Console.WriteLine("count {0} {1}", trade.Key, trade.Value.Count);
				}
			}
		}
	}
}




