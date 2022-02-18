using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
			Console.WriteLine("Type bnb/usdt, btc/usdt eth/usdt ...");
			string input = Console.ReadLine();
			string[] inputArr = input.Replace("/", "").Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			ConcurrentDictionary<string, List<Trade>> trades = new ConcurrentDictionary<string, List<Trade>>();
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
						Trade trade = new Trade();
						try
						{
							var parsedObject = JObject.Parse(e.Data);
							var jsonKeys = parsedObject.ToString();
							trade = JsonConvert.DeserializeObject<Trade>(jsonKeys);

							AddTradesToDictionary(trades, trade);
							PrintData(trades, trade);
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

			Console.ReadKey();
		}

		private static ConcurrentDictionary<string, List<Trade>> ClearDictionary(ConcurrentDictionary<string, List<Trade>> trades, int maxTradesCount)
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

		private static void AddTradesToDictionary(ConcurrentDictionary<string, List<Trade>> trades, Trade trade)
		{
			string symbol = trade.Symbol;

			if (!trades.ContainsKey(symbol))
			{
				trades.TryAdd(symbol, new List<Trade>());
			}
			trades[symbol].Add(trade);
			//return trades;
		}

		private static void PrintData(ConcurrentDictionary<string, List<Trade>> trades, Trade trade)
		{
			int tradePosition = trades.Keys.ToList().IndexOf(trade.Symbol);
			string symbol = trade.Symbol;
			string price = trade.Price;
			bool isBuyer = trade.IsBuyer;
			Console.SetCursorPosition(0, tradePosition + 2);

			if (isBuyer)
			{
				Console.BackgroundColor = ConsoleColor.Green;
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("\r{0,-10}{1,-10}", symbol, price);
				//Console.WriteLine("count {0} {1}", trade.Key, trade.Value.Count);
			}
			else
			{
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write("\r{0,-10}{1,-10}", symbol, price);
				//Console.WriteLine("count {0} {1}", trade.Key, trade.Value.Count);
			}
		}

		//private static void PrintData(ConcurrentDictionary<string, List<Trade>> trades)
		//{
		//	// TODO print updated trade separately

		//	//Console.CursorVisible = false;
		//	Console.SetCursorPosition(0, 3);

		//	foreach (var trade in trades)
		//	{
		//		string symbol = trade.Key;
		//		string price = trade.Value.Last().Price;
		//		bool isBuyer = trade.Value.Last().IsBuyer;

		//		if (isBuyer)
		//		{
		//			Console.BackgroundColor = ConsoleColor.Green;
		//			Console.ForegroundColor = ConsoleColor.White;
		//			Console.WriteLine("{0,-10}{1,-10}", symbol, price);
		//			//Console.WriteLine("count {0} {1}", trade.Key, trade.Value.Count);
		//		}
		//		else
		//		{
		//			Console.BackgroundColor = ConsoleColor.Red;
		//			Console.ForegroundColor = ConsoleColor.White;
		//			Console.WriteLine("{0,-10}{1,-10}", symbol, price);
		//			//Console.WriteLine("count {0} {1}", trade.Key, trade.Value.Count);
		//		}
		//	}
		//}
	}
}




