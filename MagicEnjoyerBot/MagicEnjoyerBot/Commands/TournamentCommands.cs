using Discord.Commands;
using MagicEnjoyerBot.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Commands
{
    public class TournamentCommands
    {
		[Group("t")]
		public class TournamentGroupModule : ModuleBase<SocketCommandContext>
		{
			[Command("create")]
			[Summary("creates a new tournament or responds that there is one currently in progress")]
			public async Task CreateTournament([Remainder] string receivedText)
			{
				await ReplyAsync(TournamentController.CreateTournament(receivedText, new KeyValuePair<ulong, ulong>(Context.Guild?.Id ?? ulong.MinValue,Context.Channel.Id)));
			}

			[Command("signup")]
			[Summary("signs up player or players")]
			public async Task SignupPlayers(string receivedText, int experience)
			{
				await ReplyAsync(TournamentController.SignupPlayer(receivedText, experience));
			}

			[Command("bulkup")]
			[Summary("signs up player or players")]
			[RequireOwner(Group = "Permission")]
			public async Task BulkSignupPlayersForTesting([Remainder]string receivedText)
			{
				//For testing, can delete
				await ReplyAsync(TournamentController.SignupPlayer(receivedText, 0));
			}

			[Command("start")]
			[Summary("starts the current tournament")]
			public async Task StartTournament()
			{

				await ReplyAsync(TournamentController.StartTournament());
			}

			[Command("report")]
			[Summary("report playername and score in x-x format")]
			public async Task ReportScore(string playerName, string score)
			{

				await ReplyAsync(TournamentController.ReportScore(playerName, score));
			}

			[Command("delete")]
			[RequireOwner(Group = "Permission")]
			public async Task DeleteTournament()
			{

				await ReplyAsync(TournamentController.DeleteTournament());
			}

			[Command("next")]
			[RequireOwner(Group = "Permission")]
			public async Task NextTournamentPhase()
			{

				await ReplyAsync(TournamentController.NextTournamentPhase());
			}
		}
	}
}
