using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace BinanceApiTask
{
	internal class BinanceWebSocket
	{
		public string url = "wss://fstream.binance.com/ /stream?streams=";
		public string input { get; }
		public List<string> pairs { get; }
		public List<WebSocket> webSockets { get; }

		public BinanceWebSocket(string input)
		{
			pairs = new List<string>();
			webSockets = new List<WebSocket>();
			CreatePairList(input);
			CreateWSocketList(pairs);
		}

		public void CreatePairList(string input)
		{
			string[] pairsToAdd = input.Replace(@"/", string.Empty).Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var pair in pairsToAdd)
			{
				if (!pairs.Contains(pair))
					pairs.Add(pair);
			}
		}

		public void CreateWSocketList(List<string> pairs)
		{
			foreach (var pair in pairs)
			{
				string pairUrl = url + pair + "@aggTrade";
				WebSocket ws = new WebSocket(pairUrl);
				webSockets.Add(ws);
			}
		}
	}
}
