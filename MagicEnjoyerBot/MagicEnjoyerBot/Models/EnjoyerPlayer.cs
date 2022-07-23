using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Models
{
    public class EnjoyerPlayer
    {
        public string Name { get; set; }

        public int Seed { get; set; }

        public int Experience { get; set; } //Experience is used in final tie breaker calc

        public string ID { get; set; }

        public double score { get; set; }

        public double sos { get; set; } //calculated when breaking ties

        public int eos { get; set; } //calculated when breaking ties

        public List<string> beat { get; set; } = new List<string>();

        public List<string> lostto { get; set; } = new List<string>();

    }
}
