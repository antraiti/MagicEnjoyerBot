using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace MagicEnjoyerBot.Controllers
{
	public static class MemeController
	{
		private static DiscordSocketClient _client;
		private static Random _random = new Random();
		public static void Initialize(DiscordSocketClient client)
		{
			_client = client;
		}

		public static string GetRandomSussy()
        {
			string sussy = "";

			List<IReadOnlyCollection<Discord.IMessage>> messageBase = _client.GetGuild(581565149987602433).GetTextChannel(933567783088824372).GetMessagesAsync().ToListAsync().Result;
			List<string> messages = new List<string>();
			foreach(var m in messageBase)
            {
				foreach(var c in m.Where(x => x.CleanContent.Contains("\"")))
                {
					messages.Add(c.CleanContent);
				}
            }
			if(messages.Count > 0) return messages[_random.Next(0, messages.Count())];
			return sussy;
        }
	}
}
