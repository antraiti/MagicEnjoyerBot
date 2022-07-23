using MagicEnjoyerBot.Models.Challonge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using MagicEnjoyerBot.Models;

namespace MagicEnjoyerBot.Controllers
{
    public static class ChallongeController
    {
        private static string _tournamentAPIKey = "DO_NOT_CHECK_IN_IF_THIS_IS_CHANGED"; //MAKE SURE YOU DONT CHECK IN THE API KEY

        public static CreateTournamentResponse CreateTournament(string tournamentName, string type, string swissrounds)
        {
            CreateTournamentResponse response = new CreateTournamentResponse();

            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "api_key", _tournamentAPIKey },
                    { "tournament[name]", tournamentName },
                    { "tournament[tournament_type]", type }
                };

                if (!string.IsNullOrEmpty(swissrounds)) values.Add("tournament[swiss_rounds]", swissrounds);

                var content = new FormUrlEncodedContent(values);
                using (HttpResponseMessage res = client.PostAsync("https://api.challonge.com/v1/tournaments.json", content).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                JObject responseObject = JObject.Parse(httpContent);

                response.tournamentID = (string)responseObject["tournament"]["id"];
                response.tournamentURL = (string)responseObject["tournament"]["full_challonge_url"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public static string StartTournament()
        {
            string response = "";

            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "api_key", _tournamentAPIKey },
                };
                var content = new FormUrlEncodedContent(values);
                using (HttpResponseMessage res = client.PostAsync("https://api.challonge.com/v1/tournaments/"+ TournamentController.GetCurrentTournamentID() + "/start.json", content).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                JObject responseObject = JObject.Parse(httpContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public static string DeleteTournament()
        {
            string response = "deleted";

            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                string url = "https://api.challonge.com/v1/tournaments/" + TournamentController.GetCurrentTournamentID() + ".json?api_key=" + _tournamentAPIKey;
                using (HttpResponseMessage res = client.DeleteAsync(url).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                JObject responseObject = JObject.Parse(httpContent);
            }
            catch (Exception ex)
            {
                response = "error";
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public static string SignupPlayers(bool useSeed = false)
        {
            //TODO: bulk query is being weird. Make single participant addition command for now to loop through instead
            string response = "";

            foreach(EnjoyerPlayer player in TournamentController.GetCurrentPlayerList().OrderBy(p=>p.Seed).ToList())
            {
                SignupSinglePlayer(TournamentController.GetCurrentTournamentID(), player, useSeed);
            }

            return response;
        }

        public static string SignupSinglePlayer(string tournamentID, EnjoyerPlayer player, bool useSeed = false)
        {
            string response = "";

            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "api_key", _tournamentAPIKey },
                    { "participant[name]", player.Name }
                };

                if (useSeed) values.Add("participant[seed]", player.Seed.ToString());

                var content = new FormUrlEncodedContent(values);

                using (HttpResponseMessage res = client.PostAsync("https://api.challonge.com/v1/tournaments/" + tournamentID + "/participants.json", content).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                JObject responseObject = JObject.Parse(httpContent);

                player.ID = (string)responseObject["participant"]["id"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public static string ReportMatchScore(string playerName, string record)
        {
            string response = "";

            //Find player id with name
            string winnerId = TournamentController.GetCurrentPlayerList().Where(p => p.Name == playerName).FirstOrDefault()?.ID ?? "missing player";

            //Send score report
            string matchID = GetOpenMatchIDByPlayerID(winnerId);

            SendMatchScore(matchID, winnerId, record);

            return response;
        }

        private static string SendMatchScore(string matchID, string winnerID, string record)
        {
            string response = "";

            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "api_key", _tournamentAPIKey },
                    { "match[scores_csv]", record },
                    { "match[winner_id]", winnerID }
                };

                var content = new FormUrlEncodedContent(values);

                string url = "https://api.challonge.com/v1/tournaments/" + TournamentController.GetCurrentTournamentID()
                    + "/matches/" + matchID + ".json";

                using (HttpResponseMessage res = client.PutAsync(url, content).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                JObject responseObject = JObject.Parse(httpContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        private static string GetOpenMatchIDByPlayerID(string playerId)
        {
            string response = "";

            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.challonge.com/v1/tournaments/" + TournamentController.GetCurrentTournamentID() 
                    + "/matches.json?api_key="+_tournamentAPIKey
                    + "&participant_id=" + playerId
                    + "&state=" + "open")
                };

                using (HttpResponseMessage res = client.SendAsync(request).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                JArray responseObject = JArray.Parse(httpContent);

                return (string)responseObject.First()["match"]["id"];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return response;
        }

        public static JArray GeMatchesByPlayerID(string playerId)
        {
            JArray responseObject = new JArray();
            string httpContent = null;
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.challonge.com/v1/tournaments/" + TournamentController.GetCurrentTournamentID()
                    + "/matches.json?api_key=" + _tournamentAPIKey
                    + "&participant_id=" + playerId)
                };

                using (HttpResponseMessage res = client.SendAsync(request).Result)
                {
                    httpContent = res.Content.ReadAsStringAsync().Result;
                }
            }

            try
            {
                responseObject = JArray.Parse(httpContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return responseObject;
        }
    }
}
