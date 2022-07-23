using Discord.WebSocket;
using MagicEnjoyerBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Controllers
{
    public static class TournamentController
    {
        private static EnjoyerTournament _currentTournament;

        private static DiscordSocketClient _client;

        public static void Initialize(DiscordSocketClient client)
        {
            _client = client;
        }

        /*
         * Creates a new tournament if none in progress
         * Responds with current tournament status if one is in progress
         */
        public static string CreateTournament(string tournamentName, KeyValuePair<ulong, ulong> guildChannelInfo)
        {
            string response = "";

            if (_currentTournament != null && _currentTournament.GetTournamentStatus() != TournamentStatus.complete)
            {
                response = "There is a tournament currently in progress. Please use end command before creating a new one.";
                response += "\nTournament Name: " + _currentTournament.GetName();
                response += "\nTournament Status: " + _currentTournament.GetTournamentStatus().ToString();
            }
            else
            {
                //Create new tournament
                _currentTournament = new ShipleyDraftController(tournamentName, guildChannelInfo); //for now we are only doing shipley drafts. Later can add other formats

                response = "New tournament created.";
            }

            return response;
        }

        public static string SignupPlayer(string playerName, int playerStrength)
        {
            string response = "";
            //Should also validate given values

            if(_currentTournament == null)
            {
                response = "Unable to signup. No currently active tournament.";
            }
            else if(_currentTournament.GetTournamentStatus() != TournamentStatus.signup)
            {
                response = "Unable to signup. Current tournament is in progress.";
            } 
            else
            {
                if (playerName.Contains(","))
                {
                    Random random = new Random();
                    List<string> players = playerName.Split(',', StringSplitOptions.TrimEntries).ToList();
                    foreach(string player in players)
                    {
                        _currentTournament.SignupPlayer(player, random.Next(0, 15));
                    }
                }
                else
                {
                    response = _currentTournament.SignupPlayer(playerName, playerStrength);
                }
            }

            return response;
        }

        public static string StartTournament()
        {
            string response = "";

            if (_currentTournament == null) response = "No current tournament";
            else if (_currentTournament.GetTournamentStatus() != TournamentStatus.signup) response = "Cannot start, current tournament is in progress";
            else return _currentTournament.Start();

            return response;
        }

        public static string NextTournamentPhase()
        {
            string response = "";

            response = _currentTournament.NextPhase();

            return response;
        }

        /*
         * Ends the current tournament and saves it to file record
         * Responds true if succesfull or no tournament in progress
         */
        public static bool EndTournament()
        {
            bool response = false;
            return response;
        }

        public static string DeleteTournament()
        {
            string response = "";

            response = ChallongeController.DeleteTournament();

            return response;
        }

        public static string ReportScore(string playerName, string score)
        {
            string response = "";

            ChallongeController.ReportMatchScore(playerName, score);

            return response;
        }

        private static void SendTournamentMessage(string message)
        {
            _client.GetGuild(_currentTournament.GetGuildChannelInfo().Key).GetTextChannel(_currentTournament.GetGuildChannelInfo().Value).SendMessageAsync(message);
        }

        public static List<EnjoyerPlayer> GetCurrentPlayerList()
        {
            return _currentTournament.GetPlayers();
        }

        public static string GetCurrentTournamentID()
        {
            return _currentTournament.GetID();
        }
    }
}
