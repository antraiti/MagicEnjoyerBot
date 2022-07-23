using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Models.Challonge
{
    public class CreateTournamentRequest
    {
        public string api_key { get; set; }

        public Tournament tournament { get; set; }
    }
}
