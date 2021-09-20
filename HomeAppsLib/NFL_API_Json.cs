using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib.API.NFL
{
    public class NFL_API_Json
    {
        public Dictionary<string, Matchup> Matchups { get; set; }
    }

    public class Matchup
    {
        public Team home { get; set; }
        public Team away { get; set; }

        public string bp { get; set; }
        public string down { get; set; }
        public string togo { get; set; }
        public string clock { get; set; }
        public string posteam { get; set; }
        public string note { get; set; }
        public string redzone { get; set; }
        public string stadium { get; set; }

        public string yl { get; set; }
        public string qtr { get; set; }

        public Media media { get; set; }
    }
    public class Team
    {
        public Dictionary<string, int?> score { get; set; }
        public string abbr { get; set; }
        public int? to { get; set; }
    }
    public class Media
    {
        public Radio radio { get; set; }
        public string tv { get; set; } // channel is it on
        public string sat { get; set; }
        public string sathd { get; set; }
    }
    public class Radio
    {
        public string home { get; set; }
        public string away { get; set; }
    }
}