using Discord;
using Discord.Commands;
using MagicEnjoyerBot.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Commands
{
    public class CardCommands
    {
		private static List<string> greetings = new List<String> { "my good friend", "the one and only", "who else but" };
		private static List<string> commanderSegways = new List<String> { "Might I interest you in", "Maybe you have heard of", "It could only be", "For you" };

		private static Random random = new Random();
		public class CardModule : ModuleBase<SocketCommandContext>
		{
			[Command("loretery")]
			[Summary("returns random card lore")]
			public async Task RandomCardLore()
			{
				string randomLore = CardController.RandomCardFlavor();
				if(randomLore.Length > 0)
                {
					await ReplyAsync(randomLore);
                }
			}

			[Command("whattoplay")]
			[Summary("returns random card lore")]
			public async Task RandomCommander()
			{
				Tuple<string, string> response = CardController.RandomCommander();
				if (response.Item1.Length > 0 && response.Item2.Length > 0)
				{
					await ReplyAsync("Ah, " + greetings[random.Next(0, greetings.Count)] + " " + Context.User.Username + ". " 
						+ commanderSegways[random.Next(0, commanderSegways.Count)] + "... " + response.Item1 + "\n" + response.Item2);
				}
			}
		}
	}
}
