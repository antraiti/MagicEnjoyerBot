using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Models.Challonge
{
    //Used for serialization of challonge requests
    //Currently using bare minimum info for requests
    public class Tournament
    {
        public string name { get; set; }

        public string tournament_type { get; set; } //Single elimination (default), double elimination, round robin, swiss

        public string id { get; set; }

        public bool open_signup = false;

        public int swiss_rounds = 0;
    }
}
