using MagicEnjoyerBot.Models;
using MagicEnjoyerBot.Models.Challonge;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Controllers
{
    //A shipley draft consists of a draft phase, a swiss phase and a double elim bracket
    public class ShipleyDraftController : EnjoyerTournament
    {
        private string _name = "ShipleyDraftDefault";
        private TournamentStatus _status = TournamentStatus.signup;
        private List<EnjoyerPlayer> _players = new List<EnjoyerPlayer>();
        private string _tournamentID;
        private ShipleyDraftStatus _draftStatus;

        private KeyValuePair<ulong, ulong> _guildChannelInfo;

        public ShipleyDraftController(string name, KeyValuePair<ulong, ulong> guildChannelInfo)
        {
            _name = name;
            _guildChannelInfo = guildChannelInfo;
        }

        public string GetName()
        {
            return _name;
        }

        public TournamentStatus GetTournamentStatus()
        {
            return _status;
        }

        public List<EnjoyerPlayer> GetPlayers()
        {
            return _players;
        }

        public string SignupPlayer(string playerName, int playerStrength)
        {
            string response = "";

            if(_players.Exists(p => p.Name.ToLower().Equals(playerName.ToLower())))
            {
                response = "Player already signed up";
            }
            else
            {
                EnjoyerPlayer newPlayer = new EnjoyerPlayer();
                newPlayer.Name = playerName;
                newPlayer.Experience = playerStrength;

                _players.Add(newPlayer);

                response = playerName + " has been signed up. \n";
                response += CurrentRosterString();
            }

            return response;
        }

        public string RemovePlayer(string playerName)
        {
            string response = "NOT IMPLEMENTED";

            return response;
        }

        public string Start()
        {
            string response = "";
            //Moves from signup/draft phase to swiss rounds. Generates new swiss bracket and responds with the URL.
            CreateTournamentResponse res = ChallongeController.CreateTournament(_name + " swiss", "swiss", "2");

            _tournamentID = res.tournamentID;
            response += res.tournamentURL;

            Random random = new Random();

            _players = _players.OrderBy(a => random.Next()).ToList();

            ChallongeController.SignupPlayers();

            ChallongeController.StartTournament();

            _draftStatus = ShipleyDraftStatus.swiss;

            return response;
        }

        /*
         * Hopefully this will work as intended.
         * We need to:
         * Make sure the result is valid.
         * Report to the current bracket.
         * If its swiss and complete, call something to begin creation of the double elim bracket
         * If its double elim and complete, call something to end and announce winner
         */
        public string ReportResult(string playerName, string result)
        {
            string response = "";

            //results should be in format winnername x-y



            return response;
        }

        public void End()
        {
            throw new NotImplementedException();
        }

        private string CurrentRosterString()
        {
            string response = "Current tournament roster:";

            foreach(EnjoyerPlayer player in _players)
            {
                response += "\n" + player.Name;
            }

            return response;
        }

        public KeyValuePair<ulong, ulong> GetGuildChannelInfo()
        {
            return _guildChannelInfo;
        }

        public string GetID()
        {
            return _tournamentID;
        }

        public string NextPhase()
        {
            string response = "";
            //Used for moving from post-swiss to double elim. calculates seeds and generates double elim
            if (_draftStatus != ShipleyDraftStatus.swiss) return "Cannot move to double elim when not in swiss";

            //first retrieve player match records
            foreach(EnjoyerPlayer player in _players)
                GeneratePlayerSwissInfo(player);

            response = GenerateSeedingForDoubleElim();
            int i = 1;
            foreach (EnjoyerPlayer p in _players.OrderBy(p=>p.Seed))
            {
                response += i + ". " + p.Name + " - " + p.score +"\n";
                i++;
            }

            CreateTournamentResponse res = ChallongeController.CreateTournament(_name, "double elimination", "");
            response += "\n" + res.tournamentURL;
            _tournamentID = res.tournamentID;


            ChallongeController.SignupPlayers(true);

            return response;
        }

        private void GeneratePlayerSwissInfo(EnjoyerPlayer player)
        {
            //grab their matches from the swiss and parse out who they beat and lost to, calculate their initial score
            JArray matches = ChallongeController.GeMatchesByPlayerID(player.ID);

            int scored = 0;

            foreach(var match in matches)
            {
                if((int)match["match"]["round"] < 4)
                {
                    if ((string)match["match"]["state"] == "complete")
                    {
                        if ((string)match["match"]["winner_id"] == player.ID)
                        {
                            player.beat.Add((string)match["match"]["loser_id"]);
                            player.score++;
                        } else
                        {
                            player.lostto.Add((string)match["match"]["winner_id"]);
                        }
                    } else
                    {
                        player.score += 0.5;
                    }
                    scored++;
                }
            }

            player.score += 3 - scored; //counts bys as a win
        }

        private string GenerateSeedingForDoubleElim()
        {
            string response = "";

            List<EnjoyerPlayer> finalList = new List<EnjoyerPlayer>();

            //first group by score. Then run tie breaker function on each group
            var grouped = _players.OrderByDescending(p => p.score).GroupBy(p => p.score);

            foreach(var group in grouped)
            {
                List<EnjoyerPlayer> sorted = BreakTie(group.ToList());
                foreach(EnjoyerPlayer player in sorted)
                {
                    finalList.Add(player);
                    player.Seed = finalList.Count();
                }
            }


            return response;
        }

        private List<EnjoyerPlayer> BreakTie(List<EnjoyerPlayer> players)
        {
            if (players.Count() <= 1) return players;

            List<EnjoyerPlayer> response = new List<EnjoyerPlayer>();

            //First split by whos played each other
            List<EnjoyerPlayer> winners = new List<EnjoyerPlayer>();
            List<EnjoyerPlayer> noadvantage = new List<EnjoyerPlayer>();
            foreach (EnjoyerPlayer player in players)
            {
                bool isAWinner = false;
                foreach(string beatplayer in player.beat)
                {
                    if(players.Exists(p => p.ID == beatplayer))
                    {
                        isAWinner = true;
                    } 
                }
                if(isAWinner) winners.Add(player);
                else noadvantage.Add(player);
            }

            //Next check strength of schedule
            foreach(EnjoyerPlayer player in noadvantage)
            {
                foreach(string beatplayer in player.beat)
                {
                    EnjoyerPlayer temp = _players.Where(p => p.ID == beatplayer).First();
                    player.sos += temp.score;
                    player.eos += temp.Experience;
                }
                foreach (string losttoplayer in player.lostto)
                {
                    EnjoyerPlayer temp = _players.Where(p => p.ID == losttoplayer).First();
                    player.sos += temp.score;
                    player.eos += temp.Experience;
                }
            }

            List<EnjoyerPlayer> sortedNoad = new List<EnjoyerPlayer>();
            var sosgrouped = noadvantage.OrderByDescending(p => p.sos).GroupBy(p => p.sos);

            //finally check if any groups need to be checked for experience of schedule
            foreach(var group in sosgrouped)
            {
                if(group.ToList().Count() <= 1)
                {
                    sortedNoad.Add(group.First());
                }
                else
                {
                    //tie from SoS, calculate and order by EoS to do final break
                    List<EnjoyerPlayer> expSorted = group.ToList().OrderByDescending(p => p.eos).ToList();
                    foreach (EnjoyerPlayer sortedPlayer in expSorted)
                        sortedNoad.Add(sortedPlayer);
                }
            }

            winners = BreakTie(winners); //recursive call to use later filters on winners

            foreach(EnjoyerPlayer p in winners)
                response.Add(p);
            foreach (EnjoyerPlayer p in sortedNoad)
                response.Add(p);

            return response;
        }
    }

    public enum ShipleyDraftStatus
    {
        signup,
        swiss,
        doubleElim,
        complete
    }
}
