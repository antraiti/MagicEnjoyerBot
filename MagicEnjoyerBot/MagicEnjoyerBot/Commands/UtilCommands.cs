using Discord.Commands;
using MagicEnjoyerBot.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Commands
{
    public class UtilCommands
    {
		public class UtilModule : ModuleBase<SocketCommandContext>
		{
			[Command("admin")]
			[Summary("Adds as admin")]
			[RequireOwner]
			public async Task AddAsAdmin()
			{
				CacheController.infoCache.Admins.Add(Context.User.Id);
				await ReplyAsync("Admin'd");
			}

			[Command("grabchannelinfo")]
			[Summary("grabs and prints info to console")]
			[RequireOwner]
			public async Task GrabChannelInfo()
			{
				Console.WriteLine(Context.Guild.Id + " " + Context.Channel.Id);
			}
		}
	}
}
