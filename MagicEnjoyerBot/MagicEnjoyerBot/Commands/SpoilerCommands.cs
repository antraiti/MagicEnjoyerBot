using Discord;
using Discord.Commands;
using MagicEnjoyerBot.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Commands
{
    //All commands related to the spoiler functionality
    public class SpoilerCommands
    {
        [Group("spoil")]
        public class SpoilGroupModule : ModuleBase<SocketCommandContext>
        {
            [Command("overhere")]
            [Summary("Sets current text channel to output for spoilers.")]
            [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
            [RequireOwner(Group = "Permission")]
            public async Task SetSpoilLocation()
            {
                SpoilerController.SetSubscriberChannel(Context.Guild.Id, Context.Channel.Id);
                await ReplyAsync("Ah, " + Context.Channel.Name + " it is.");
            }

            [Command("unsub")]
            [Summary("Unsubscribes guild from spoiler alerts.")]
            public async Task UnsubsribeFromSpoilers()
            {
                SpoilerController.UnsubscriberChannel(Context.Guild.Id);
                await ReplyAsync("Ah, sadness.");
            }

            [Command("latest")]
            [Summary("Returns latest card from spoilers.")]
            public async Task ReturnLatestSpoil()
            {
                await ReplyAsync(SpoilerController.GetLatest());
            }

            [Command("until")]
            [Summary("Returns spoiled cards up until listed one.")]
            public async Task ReturUntilSpoil(string url)
            {
                //This currently will need to be upgraded to use 2 inputs (1 for the set and 1 for the card to spoil until)
                await ReplyAsync("This command needs to be updated");
                //await SpoilerController.GetUntil(url, Context);
            }

            [Command("interval")]
            [Summary("Sets current spoil interval in seconds GLOBALLY. Requires permissions.")]
            [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
            [RequireOwner(Group = "Permission")]
            public async Task SetSpoilInterval(int seconds)
            {
                SpoilerController.UpdateInterval(TimeSpan.FromSeconds(seconds));
                await ReplyAsync("Interval set to " + seconds + " seconds");
            }

            [Command("set")]
            [Summary("adds set to list of sets to monitor, include url of latest seen card there as well")]
            [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
            [RequireOwner(Group = "Permission")]
            public async Task SetSpoilSet([Summary("Set page url")]string setURL, [Summary("url of latest card spoiled in that set")] string latestURL)
            {
                SpoilerController.AddSpoilerSetWithLatest(setURL, latestURL);
                await ReplyAsync("Added/updated set");
            }
        }
    }
}
