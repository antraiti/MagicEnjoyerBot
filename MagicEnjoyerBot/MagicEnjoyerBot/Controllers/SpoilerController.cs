﻿using HtmlAgilityPack;
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
    public static class SpoilerController
    {
		//Running with static classes for the controllers for now, not sure if its gunna hold tho
		
		//These are used for controlling the job task (not ideal) we should eventually just switch to managing with a task factory and things
		private static bool _cancelRun = false;
		private static bool _clearToRun = true;
		
		private static DiscordSocketClient _client;

		public static void Initialize(DiscordSocketClient client)
        {
			_client = client;
		}

		public static void StartSpoilJob()
        {
			SpoilerJob(TimeSpan.FromSeconds(CacheController.infoCache.SpoilerIntervalSeconds));
		}


		public static void Save()
        {
            CacheController.SaveList();
        }

		public static void SetSubscriberChannel(ulong GuildID, ulong ChannelID)
        {
			if(CacheController.infoCache.SubsGuildXChannel.ContainsKey(GuildID))
				CacheController.infoCache.SubsGuildXChannel[GuildID] = ChannelID;
			else
				CacheController.infoCache.SubsGuildXChannel.Add(GuildID, ChannelID);

			CacheController.SaveList();
        }

		public static void UnsubscriberChannel(ulong GuildID)
		{
			if(CacheController.infoCache.SubsGuildXChannel.ContainsKey(GuildID))
            {
				CacheController.infoCache.SubsGuildXChannel.Remove(GuildID);
            }

			CacheController.SaveList();
		}

		public static void AddSpoilerSetWithLatest(string setURL, string latestURL)
		{
			if(CacheController.infoCache.SpoilUrlsXSeen.ContainsKey(setURL))
				CacheController.infoCache.SpoilUrlsXSeen[setURL] = latestURL;
			else
				CacheController.infoCache.SpoilUrlsXSeen.Add(setURL, latestURL);

			CacheController.SaveList();
		}

		//this will have to be changed away from using hardcoded url
		public static string GetLatest()
		{
			using (WebClient web1 = new WebClient())
			{
				Console.WriteLine("starting spoil search");
				string data = web1.DownloadString("https://www.magicspoiler.com/mtg-set/streets-of-new-capenna/");

				Console.WriteLine("downloaded");
				HtmlDocument htmlSnippet = new HtmlDocument();

				Console.WriteLine("loading");
				htmlSnippet.LoadHtml(data);

				Console.WriteLine("searching");
				HtmlNode latestBox = htmlSnippet.GetElementbyId("MS Spoilers M");

				Console.WriteLine("found");
				string latestHref = latestBox.Descendants("a").FirstOrDefault().Attributes["href"].Value;

				Console.WriteLine(latestHref);
				string data2 = web1.DownloadString(latestHref);

				Console.WriteLine("downloaded latest page");
				htmlSnippet.LoadHtml(data2);
				HtmlNode latestMain = htmlSnippet.GetElementbyId("main");

				Console.WriteLine("main found");
				string latestImage = latestMain.Descendants("img").Where(x => x.Attributes["src"] != null).First().Attributes["src"].Value;

				Console.WriteLine("image found");
				return latestImage;
			}
		}

		public static async Task GetUntil(string url, SocketCommandContext Context)
		{
			List<string> spoilers = new List<string>();
			using (WebClient web1 = new WebClient())
			{
				Console.WriteLine("starting");
				string data = web1.DownloadString("https://www.magicspoiler.com/mtg-set/streets-of-new-capenna/");

				Console.WriteLine("downloaded");
				HtmlDocument htmlSnippet = new HtmlDocument();

				Console.WriteLine("loading");
				htmlSnippet.LoadHtml(data);

				Console.WriteLine("searching");
				HtmlNode latestBox = htmlSnippet.GetElementbyId("MS Spoilers M");

				Console.WriteLine("found");
				string currentHref = latestBox.Descendants("a").FirstOrDefault().Attributes["href"].Value;

				while(currentHref != url)
                {
					//TODO: Fix this using logic from scrubby
					string data2 = web1.DownloadString(currentHref);
					htmlSnippet = new HtmlDocument();
					htmlSnippet.LoadHtml(data2);
					HtmlNode latestMain = htmlSnippet.GetElementbyId("main");
					string latestImage = latestMain.Descendants("img").Where(x => x.Attributes["src"] != null).First().Attributes["src"].Value;

					await Context.Channel.SendMessageAsync(latestImage);
				}
			}
		}

		public static void UpdateInterval(TimeSpan interval)
        {
			//this can be changed later when we switch to task factory 
			_cancelRun = true;
			while (!_clearToRun) Task.Delay(500);
			SpoilerJob(interval);
		}

        public static async Task SpoilerJob(TimeSpan interval)
        {
			_cancelRun = false;
			while (!_cancelRun)
			{
				Console.WriteLine("Checking for latest spoilers: " + DateTime.Now.ToString());
				foreach(KeyValuePair<string, string> uxl in CacheController.infoCache.SpoilUrlsXSeen)
                {
					await CheckAndSendLatestForURL(uxl.Key, uxl.Value);
                }
				await Task.Delay(interval);
			}
			_clearToRun = true;
		}

		private static async Task CheckAndSendLatestForURL(string url, string latest)
        {
			try
			{
				Console.WriteLine("Checking for latest spoilers: " + DateTime.Now.ToString());
				using (WebClient web1 = new WebClient())
				{
					List<string> usedUrls = new List<string>();
					Console.WriteLine("Current latest: " + latest);
					string data = web1.DownloadString(url);
					Console.WriteLine("downloaded");
					HtmlDocument htmlSnippet = new HtmlDocument();
					Console.WriteLine("loading");
					htmlSnippet.LoadHtml(data);
					Console.WriteLine("searching");
					HtmlNode latestBox = htmlSnippet.GetElementbyId("MS Spoilers M");
					Console.WriteLine("found");
					List<HtmlNode> latestImages = latestBox.Descendants("a").ToList();
					string latestHref = latestImages.FirstOrDefault().Attributes["href"].Value;
					if (latestHref != latest)
					{
						Console.WriteLine("!New Spoilers Found!");
						//new stuff
						string oldHref = latest;
						Console.WriteLine(oldHref);
						CacheController.infoCache.SpoilUrlsXSeen[url] = latestHref;
						CacheController.SaveList();

						//send images since latest
						bool found = false;
						foreach (HtmlNode node in latestBox.Descendants())
						{
							if (!found && node.HasAttributes && node.Name == "a")
							{
								string currentHref = node.Attributes["href"].Value;
								if (currentHref.Substring(0, 2) == "ht" && currentHref.Contains("mtg-spoiler"))
								{
									Console.WriteLine("node reading " + currentHref);
									if (currentHref != oldHref)
									{
										if (!usedUrls.Contains(currentHref))
										{
											usedUrls.Add(currentHref);
											Console.WriteLine(currentHref);
											string data2 = web1.DownloadString(currentHref);
											Console.WriteLine("downloaded latest page");
											htmlSnippet = new HtmlDocument();
											htmlSnippet.LoadHtml(data2);
											HtmlNode latestMain = htmlSnippet.GetElementbyId("main");
											Console.WriteLine("main found");
											string latestImage = latestMain.Descendants("img").Where(x => x.Attributes["src"] != null).First().Attributes["src"].Value;
											Console.WriteLine("image found");
											
											//Send image to all subs
											await SendMessageToSubscribers(latestImage);

											//Check if there is translation text to send
											HtmlNode textArea = latestMain.Descendants("div").Where(x => x.GetClasses().Contains("c-content")).FirstOrDefault();
											foreach (HtmlNode n in textArea.Descendants("p"))
											{
												if (n.ChildNodes[0] != null && n.ChildNodes[0].Name == "#text")
												{
													await SendMessageToSubscribers(n.InnerHtml);
												}
											}

										}
									}
									else
									{
										Console.WriteLine("all done");
										found = true;
									}
								}
							}
						}
					}
					Console.WriteLine("Nothing new...");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("CAUGHT EXCEPTION: " + ex.ToString());
			}
		}

		private static async Task SendMessageToSubscribers(string message)
        {
			foreach (KeyValuePair<ulong, ulong> subsList in CacheController.infoCache.SubsGuildXChannel)
			{
				Console.WriteLine("Sending message to channel " + subsList.Key);
				await _client.GetGuild(subsList.Key).GetTextChannel(subsList.Value).SendMessageAsync(message);
			}
		}
    }
}
