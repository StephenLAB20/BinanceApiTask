using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;


namespace BinanceApiTask
{
	internal class Program
	{
		public static void Main()
		{
			Console.Write("Type the pairs (ex.: bnb/usdt, btc/usdt eth/usdt): ");
			string input = Console.ReadLine();
			Console.WriteLine("{0,-10} {1,-10} {2,-10}", "Pair", "Price", "Quantity");
			string[] pairs = input.Replace("/", "").Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			ConcurrentDictionary<string, List<Trade>> trades = new ConcurrentDictionary<string, List<Trade>>();
			int maxTradesCount = 10000;

			foreach (var pair in pairs)
			{
				new Thread(() =>
				{
					ApplyConnection(pair, trades);
				}
			).Start();
			}

			new Thread(() =>
			{
				while (trades.Count > 0)
				{
					ClearDictionary(trades, maxTradesCount);
					Thread.Sleep(60000);
				}
			}
			).Start();

			Console.ReadKey();
		}

		private static async void ApplyConnection(string pair, ConcurrentDictionary<string, List<Trade>> trades)
		{
			string url = $"wss://fstream.binance.com/ws/{pair}@aggTrade";
			var wsc = new ClientWebSocket();
			await wsc.ConnectAsync(new Uri(url), CancellationToken.None);
			Trade trade = new Trade();
			var buffer = new ArraySegment<byte>(new byte[1024]);

			while (true)
			{
				WebSocketReceiveResult result = await wsc.ReceiveAsync(buffer, CancellationToken.None);
				if (result.CloseStatus.HasValue)
					break;
				string str = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);

				try
				{
					if (str != null)
					{
						var parsedObject = JObject.Parse(str);
						var jsonKeys = parsedObject.ToString();
						trade = JsonConvert.DeserializeObject<Trade>(jsonKeys);
						AddTrade(trades, trade);
						PrintTrade(trades, trade);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		private static void ClearDictionary(ConcurrentDictionary<string, List<Trade>> trades, int maxTradesCount)
		{
			foreach (var trade in trades)
			{
				if (trade.Value.Count > maxTradesCount)
				{
					trade.Value.RemoveRange(0, trade.Value.Count - maxTradesCount);
				}
			}
		}

		private static void AddTrade(ConcurrentDictionary<string, List<Trade>> trades, Trade trade)
		{
			string symbol = trade.Symbol;

			if (!trades.ContainsKey(symbol))
			{
				trades.TryAdd(symbol, new List<Trade>());
			}
			trades[symbol].Add(trade);
		}

		private static void PrintTrade(ConcurrentDictionary<string, List<Trade>> trades, Trade trade)
		{
			int tradePosition = trades.Keys.ToList().IndexOf(trade.Symbol);
			string symbol = trade.Symbol;
			string price = trade.Price;
			double quantity = trade.Quantity;
			bool isBuyer = trade.IsBuyer;
			Console.CursorVisible = false;
			Console.SetCursorPosition(0, tradePosition + 2);
			if (isBuyer)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("\r{0,-10} {1,-10} {2,-10}", symbol, price, quantity);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("\r{0,-10} {1,-10} {2,-10}", symbol, price, quantity);
			}
		}
	}
}




