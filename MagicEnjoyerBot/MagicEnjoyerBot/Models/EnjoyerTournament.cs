using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Models
{
    public interface EnjoyerTournament
    {
        public string GetName();

        public KeyValuePair<ulong, ulong> GetGuildChannelInfo();

        public TournamentStatus GetTournamentStatus();

        public List<EnjoyerPlayer> GetPlayers();

        public string SignupPlayer(string playerName, int playerStrength);

        public string RemovePlayer(string playerName);

        public string Start();

        public string ReportResult(string playerName, string result);

        public void End();

        public string GetID();

        public string NextPhase(); //used for multi phase tournaments

    }

    public enum TournamentStatus
    {
        signup,
        inprogress,
        complete
    }
}
