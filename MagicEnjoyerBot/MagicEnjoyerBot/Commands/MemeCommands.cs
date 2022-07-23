using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MagicEnjoyerBot.Controllers;

namespace MagicEnjoyerBot.Commands
{
	//These should be dummy commands with no real access to anything for testing/learning
	//All commands need to be public and extend ModuleBase
	public class MemeCommands
	{
		public class MemeModule : ModuleBase<SocketCommandContext>
		{
			[Command("sussy")]
			[Summary("returns a sussy")]
			public async Task SimpleSussy()
			{
				string sussy = MemeController.GetRandomSussy();
				if (sussy.Length > 0) await ReplyAsync(sussy);
			}
		}
	}
}
