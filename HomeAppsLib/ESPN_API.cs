using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib.API.ESPN
{
    public class Feed
    {
        public Event[] events { get; set; }
        public Week week;        
    }

    public class Event
    {
        public string id { get; set; }
        public Status status { get; set; }
        public Competition[] competitions { get; set; }
        public DateTime date { get; set; }        
    }
    public class Status
    {
        public Type type { get; set; }       
        public string displayClock { get; set; }
        public int period { get; set; }
    }
    public class Type
    {
        public string state { get; set; }
        public string shortDetail { get; set; }
    }
    public class Competition
    {
        public Competitor[] competitors { get; set; }
        public Broadcast[] broadcasts { get; set; }
        public Venue venue { get; set; }
    }

    public class Competitor
    {
        public string homeAway { get; set; }
        public Team team { get; set; }
        public string score { get; set; }
        
    }
    public class Team
    {
        public string shortDisplayName { get; set; }
    }
    public class Broadcast
    {
        public string market { get; set; }
        public string[] names { get; set; }
    }
    public class Week
    {
        public int number { get; set; }
    }
    public class Venue
    {
        public string fullName { get; set; }
    }
}