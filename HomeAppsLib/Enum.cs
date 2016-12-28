using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib
{
    public enum WeekTypes
    {
        Empty = -1,
        PreSeason = 0,
        RegularSeason = 1,
        PostSeason = 2
    }
    public enum EmailSubscriptionType
    {
        NFLPicksReminderEmail = 100,
        DailyForumRecap = 200,
        MLBPicksReminderEmail = 300
    }
    public enum Sports
    {
        NFL = 100,
        MLB = 200
    }
}
