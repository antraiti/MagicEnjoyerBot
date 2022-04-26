using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MagicEnjoyerBot.Controllers;
using System.Net;
using System.Configuration;

namespace MagicEnjoyerBot
{
	public class Program
	{
		private DiscordSocketClient _client;
		private CommandHandler _commandHandler;
		private CommandService _commandService;
		private string _token = "PUT_YOUR_BOT_TOKEN_HERE"; //DO NOT CHECK IN WITH YOUR BOT TOKEN

		static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			Console.WriteLine("Starting Up..."); //TODO: Create a better logging/output system
			var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
			_client = new DiscordSocketClient(_config);
			Console.WriteLine("Getting client...");
			await _client.LoginAsync(TokenType.Bot, _token);
			await _client.StartAsync();

			await InitializeControllers(); //make sure to setup controllers before commands are live

			_commandService = new CommandService();
			_commandHandler = new CommandHandler(_client, _commandService);

			_commandHandler.InstallCommandsAsync(); //loads in all the auto-found commands

			_client.MessageUpdated += MessageUpdated;
			_client.Ready += () =>
			{
				Console.WriteLine("Bot is connected!");
				StartJobs();
				return Task.CompletedTask;
			};

			await Task.Delay(-1); //infinite block
		}

		private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
		{
			var message = await before.GetOrDownloadAsync();
		}

		private async Task InitializeControllers()
		{
			//definitely do cache first potentially we should switch this out to be sure it runs before the others
			CacheController.Initialize();

			SpoilerController.Initialize(_client);
			MemeController.Initialize(_client);
		}

		private async Task StartJobs()
        {
			SpoilerController.StartSpoilJob();
		}
	}
}