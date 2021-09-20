using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using HomeAppsLib;

namespace HomeWebApp
{
    public class CacheHelper
    {
        #region Keys
        public const string GridViewTheBetDataSourceKey = "GridViewTheBetDataSource";
        public const string GridViewMatchupsDataSourceKey = "GridViewMatchupsDataSource";
        public const string GridViewCummmulativeStatsDataSourceKey = "GridViewCummmulativeStatsDataSource";
        public const string GridViewWeeklyStatsDataSourceKey = "GridViewStatsDataSource";

        public const int CacheExpiration = 3600; // 1 hour (seconds)
        #endregion

        #region Weekly stats
        public static DataTable GetCachedGridViewWeeklyStatsDataSource(int week, bool clearCache = false, string overrideKey = null)
        {
            string key = overrideKey ?? GridViewWeeklyStatsDataSourceKey + "-" + week;
            if (!string.IsNullOrEmpty(overrideKey))
            {
                string[] parts = overrideKey.Split('-');
                week = Convert.ToInt32(parts[1]);
            }

            var obj = HttpContext.Current.Cache[key];
            if (clearCache && obj != null)
            {
                HttpContext.Current.Cache.Remove(key);
                obj = null;
            }

            if (obj == null)
            {
                HttpContext.Current.Cache.Insert(key, GetDatabaseGridViewWeeklyStatsDataSource(week), null, DateTime.Now.AddSeconds(CacheExpiration), TimeSpan.Zero);
                obj = HttpContext.Current.Cache[key];
            }
            return (DataTable)obj; 
        }
        private static DataTable GetDatabaseGridViewWeeklyStatsDataSource(int week)
        {
            var data = Common.DBModel();

            var grid = from t in data.NFL_userPicks
                       where t.week == week
                       select new
                       {
                           Person = t.username,
                           Percentage = ComputePercentage(t.picks, week),
                           Ratio = ComputeRatio(t.picks, week),
                           PickDate = t.pick_dt.ToShortDateString(),
                           Picks = t.picks
                       };

            List<WeeklyStatsGridObject> gridObjects = new List<WeeklyStatsGridObject>();
            foreach (var item in grid)
            {
                WeeklyStatsGridObject gridObject = new WeeklyStatsGridObject();
                gridObject.Percentage = item.Percentage;
                gridObject.Ratio = item.Ratio;
                gridObject.Person = item.Person;
                gridObject.PickDate = item.PickDate;
                gridObject.Picks = item.Picks;
                gridObjects.Add(gridObject);
            }

            if (grid.Count() > 0)
            {
                int count = grid.First().Picks.Split(',').Length;
                System.Data.DataTable gridTable = ContructGridTable(count);

                decimal percentage = 0;
                int ranking = 1;
                int pickCount = 1;
                int i;

                foreach (var item in gridObjects.OrderByDescending(x => x.Percentage))
                {
                    System.Data.DataRow dr = gridTable.NewRow();
                    dr["Person"] = item.Person;
                    dr["Percent"] = item.Percentage;
                    dr["Ratio"] = item.Ratio;
                    dr["PickDate"] = item.PickDate;

                    if (pickCount == 1)
                    {
                        dr["Ranking"] = ranking;
                    }
                    else
                    {
                        if (item.Percentage != percentage) ranking = pickCount;
                        dr["Ranking"] = ranking;
                    }
                    percentage = item.Percentage;
                    pickCount++;

                    i = 1;
                    foreach (var matchup in data.NFL_Matchups.Where(x => x.week == week).OrderBy(x => x.eid))
                    {
                        dr[i.ToString()] = item.Picks.Contains(matchup.home) ? matchup.home : matchup.away;
                        i++;
                    }

                    gridTable.Rows.Add(dr);

                }

                System.Data.DataRow blankRow = gridTable.NewRow();
                System.Data.DataRow winnerRow = gridTable.NewRow();
                winnerRow["Person"] = "Winners";
                i = 1;
                foreach (var matchup in data.NFL_Matchups.Where(x => x.week == week).OrderBy(x => x.eid))
                {
                    winnerRow[i.ToString()] = matchup.winner ?? "??";
                    i++;
                }

                gridTable.Rows.Add(blankRow);
                gridTable.Rows.Add(winnerRow);
                return gridTable;
            }
            else
                return new DataTable();
        }
        class WeeklyStatsGridObject
        {
            public string Person { get; set; }
            public string PickDate { get; set; }
            public decimal Percentage { get; set; }
            public string Ratio { get; set; }
            public string Picks { get; set; }
        }
        private static decimal ComputePercentage(string picksString, int week)
        {
            string[] picks = picksString.Split(',');
            int totalPicks = picks.Length;
            int goodPicks = Common.DBModel().NFL_Matchups.Count(x => x.week == week && picks.Contains(x.winner));

            if (goodPicks > 0)
                totalPicks -= Common.DBModel().NFL_Matchups.Count(x => x.week == week && x.winner == null);

            decimal percent = Math.Round((decimal)goodPicks / totalPicks * 100, 1);
            return percent;
            //return percent + "%"; 
        }
        private static string ComputeRatio(string picksString, int week)
        {
            string[] picks = picksString.Split(',');
            int totalPicks = picks.Length;
            int goodPicks = Common.DBModel().NFL_Matchups.Count(x => x.week == week && picks.Contains(x.winner));

            //if (goodPicks > 0)
            totalPicks -= Common.DBModel().NFL_Matchups.Count(x => x.week == week && x.winner == null);

            return goodPicks + "/" + totalPicks;
        }
        private static DataTable ContructGridTable(int count)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("Person");
            dt.Columns.Add("Percent");
            dt.Columns.Add("Ratio");
            dt.Columns.Add("Ranking");
            dt.Columns.Add("PickDate");

            for (int i = 1; i <= count; i++)
                dt.Columns.Add(i.ToString());

            return dt;
        }
        #endregion

        #region Cummulative stats
        public static DataTable GetCachedCummmulativeGridViewStatsDataSource(int week, int? yearId, int[] weekTypes, bool clearCache = false, string overrideKey = null)
        {
            string key = overrideKey ?? GridViewCummmulativeStatsDataSourceKey + "-" + week + "-" + yearId + "-" + string.Join("|", weekTypes.Select(w => w.ToString()).ToArray());
            if (!string.IsNullOrEmpty(overrideKey))
            {
                string[] parts = overrideKey.Split('-');
                week = Convert.ToInt32(parts[1]);

                if (!string.IsNullOrEmpty(parts[2]))
                    yearId = Convert.ToInt32(parts[2]);

                if (!string.IsNullOrEmpty(parts[3]))
                    weekTypes = ToIntegerArray(parts[3]);
            }

            var obj = HttpContext.Current.Cache[key];
            if (clearCache && obj != null)
            {
                HttpContext.Current.Cache.Remove(key);
                obj = null;
            }

            if (obj == null)
            {
                HttpContext.Current.Cache.Insert(key, GetDatabaseCummmulativeGridViewStatsDataSource(week, yearId, weekTypes), null, DateTime.Now.AddSeconds(CacheExpiration), TimeSpan.Zero);
                obj = HttpContext.Current.Cache[key];
            }
            return (DataTable)obj;
        }

        private static int[] ToIntegerArray(string pipeDelimited)
        {
            List<int> result = new List<int>();
            foreach(string str in pipeDelimited.Split('|'))
                try { result.Add(Convert.ToInt32(str.Trim())); }
                catch { }
            return result.ToArray();
        }
        private static DataTable GetDatabaseCummmulativeGridViewStatsDataSource(int week, int? yearId, int[] weekTypes)
        {
            var data = Common.DBModel();
            System.Data.DataTable allStats = new System.Data.DataTable();
            allStats.Columns.Add("Name");
            allStats.Columns.Add("Percent");
            allStats.Columns.Add("Ratio");

            int[] weeksInYear;
            if (yearId.HasValue)
                weeksInYear = Common.DBModel().NFL_weeks.Where(x => x.year_id == yearId).Select(x => x.week).ToArray();
            else
            {
                int sportId = Common.GetCurrentSportId();
                int[] yearIds = Common.DBModel().NFL_years.Where(x => x.sport_id == sportId).Select(x => x.id).ToArray();
                weeksInYear = Common.DBModel().NFL_weeks.Where(x => yearIds.Contains(x.year_id.Value)).Select(x => x.week).ToArray();
            }
            
            foreach (string user in Common.DBModel().users.Select(x => x.name).ToArray())
            {
                if (Common.DBModel().NFL_userPicks.Count(x => x.username == user && (x.NFL_week.year_id == yearId || !yearId.HasValue) && weekTypes.Contains(x.NFL_week.weekTypeId.Value)) > 0)
                {
                    string ratio = CalculateTotalRatio(Common.DBModel().NFL_userPicks.Where(x => x.username == user && x.week <= week && weeksInYear.Contains(x.week) && weekTypes.Contains(x.NFL_week.weekTypeId.Value)));

                    System.Data.DataRow dr = allStats.NewRow();
                    dr["Name"] = user;
                    dr["Ratio"] = ratio;
                    double numerator = Convert.ToDouble(ratio.Split('/')[0]);
                    int denominator = Convert.ToInt32(ratio.Split('/')[1]);
                    dr["Percent"] = denominator == 0 ? 0 : Math.Round(numerator / denominator * 100, 1);
                    allStats.Rows.Add(dr);
                }
            }

            return allStats;
        }
        private static string CalculateTotalRatio(IQueryable<db.NFL_userPick> picks, bool onlyActiveTeams = false)
        {
            int totalPicks = 0;
            int totalCorrectPicks = 0;

            foreach (var item in picks)
            {
                string[] picksArray = item.picks.Split(',');
                if (onlyActiveTeams)
                {
                    string[] activeTeams = Common.DBModel().NFL_teams.Where(x => x.display_flag).Select(x => x.name).ToArray();
                    totalPicks += Common.DBModel().NFL_Matchups.Count(x => activeTeams.Contains(x.away) && activeTeams.Contains(x.home) && x.winner != null && x.week == item.week);
                    totalCorrectPicks += Common.DBModel().NFL_Matchups.Count(x => activeTeams.Contains(x.away) && activeTeams.Contains(x.home) && x.week == item.week && (picksArray.Contains(x.winner)));
                }
                else
                {
                    totalPicks += Common.DBModel().NFL_Matchups.Count(x => x.winner != null && x.week == item.week);
                    totalCorrectPicks += Common.DBModel().NFL_Matchups.Count(x => x.week == item.week && (picksArray.Contains(x.winner)));
                }
            }

            return totalCorrectPicks + "/" + totalPicks;
        }
        #endregion

        // this (The Bet) appears most expensive, 25 seconds on first test in prod 11/26/14
        #region The Bet
        public static DataTable GetCachedGridViewTheBetDataSource(int yearId, bool clearCache = false, string overrideKey = null)
        {
            string key = overrideKey ?? GridViewTheBetDataSourceKey + "-" + yearId;
            if (!string.IsNullOrEmpty(overrideKey))
            {
                yearId = Convert.ToInt32(overrideKey.Split('-')[1]); 
            }

            var obj = HttpContext.Current.Cache[key];
            if (clearCache && obj != null)
            {
                HttpContext.Current.Cache.Remove(key);
                obj = null;
            }

            if (obj == null)
            {
                HttpContext.Current.Cache.Insert(key, GetDatabaseGridViewTheBetDataSource(yearId), null, DateTime.Now.AddSeconds(CacheExpiration), TimeSpan.Zero);
                obj = HttpContext.Current.Cache[key];
            }
            return (DataTable)obj;
        }
        private static DataTable GetDatabaseGridViewTheBetDataSource(int yearId)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("W-L");
            dt.Columns.Add("$$");
            dt.Columns.Add("Breakdown");
            dt.Columns.Add("Money", typeof(decimal)); // for sorting purposes only, remove later

            foreach (string user in Authentication.BetUsers(yearId))
            {
                string ratio = CacheHelper.CalculateTotalRatio(Common.DBModel().NFL_userPicks.Where(x => x.username == user && x.NFL_week.year_id == yearId && x.NFL_week.weekTypeId != (int)WeekTypes.PreSeason && x.NFL_week.weekTypeId != (int)WeekTypes.Empty), true);

                decimal num = Convert.ToInt32(ratio.Split('/')[0]);

                decimal value = 0;
                string breakdown = string.Empty;
                foreach (string otherUser in Authentication.BetUsers(yearId).Where(x => x != user))
                {
                    string theirRatio = CacheHelper.CalculateTotalRatio(Common.DBModel().NFL_userPicks.Where(x => x.username == otherUser && x.NFL_week.year_id == yearId && x.NFL_week.weekTypeId != (int)WeekTypes.PreSeason && x.NFL_week.weekTypeId != (int)WeekTypes.Empty), true);
                    decimal theirNum = Convert.ToInt32(theirRatio.Split('/')[0]);

                    decimal amount = (num - theirNum) * Common.GetBetUnitAmount(yearId);
                    string amountString = amount.ToString();
                    if (!amountString.StartsWith("-"))
                        amountString = "+" + amountString;

                    breakdown += "(" + otherUser + " " + amountString + ")";
                    value += amount;
                }
                string valueString = value.ToString();
                if (!valueString.StartsWith("-"))
                    valueString = "+" + valueString;

                System.Data.DataRow dr = dt.NewRow();
                dr["Name"] = user;
                dr["$$"] = "<span title='" + breakdown + "'>" + valueString + "</span>";
                dr["Breakdown"] = breakdown;
                dr["Money"] = value;
                dr["W-L"] = ratio.Split('/')[0] + " & " + (Convert.ToInt32(ratio.Split('/')[1]) - Convert.ToInt32(ratio.Split('/')[0]));

                //dr["Breakdown"] = breakdown;
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion

        #region Matchups
        public static DataTable GetCachedGridViewMatchupsDataSource(int week, bool clearCache = false, string overrideKey = null)
        {
            string key = overrideKey ?? GridViewMatchupsDataSourceKey + "-" + week;
            if (!string.IsNullOrEmpty(overrideKey))
            {
                week = Convert.ToInt32(overrideKey.Split('-')[1]); 
            }

            var obj = HttpContext.Current.Cache[key];
            if (clearCache && obj != null)
            {
                HttpContext.Current.Cache.Remove(key);
                obj = null;
            }

            if (obj == null)
            {
                HttpContext.Current.Cache.Insert(key, GetDatabaseGridViewMatchupsDataSource(week), null, DateTime.Now.AddSeconds(CacheExpiration), TimeSpan.Zero);
                obj = HttpContext.Current.Cache[key];
            }
            return (DataTable)obj;
        }
        private static DataTable GetDatabaseGridViewMatchupsDataSource(int week)
        {
            var data = Common.DBModel();
            string[] usersToAdd = new string[0];
            if (Common.getUsersToAddToMatchupsGrid() != null)
                usersToAdd = Common.getUsersToAddToMatchupsGrid();
            System.Data.DataTable matchups = ConstructMatchupsDT(usersToAdd);

            var dbMatchups = data.NFL_Matchups.Where(x => x.week == week).ToList();

            try { dbMatchups = dbMatchups.OrderBy(d => Convert.ToDateTime(d.scheduled)).ToList(); }
            catch { dbMatchups = dbMatchups.OrderBy(d => d.scheduled).ToList(); }

            bool showLive = dbMatchups.Any(m => m.live_update != null);

            if (showLive)
                matchups.Columns.Add("Now");

            foreach (var t in dbMatchups)
            {
                System.Data.DataRow row = matchups.NewRow();

                //row["Date"] = DateFromEID(t.eid);
                row["Date"] = t.scheduled;
                row["Channel"] = t.channel?.Split(' ')[0]; // just because NFL NETWORK is so fucking long
                row["Visitor"] = t.away;
                row["VisitorScore"] = t.away_score;
                row["Home"] = t.home;
                row["HomeScore"] = t.home_score;
                row["Status"] = t.status;
                //row["YourPick"] = DidUserMakeThisPick(t.home, t.week) ? t.home : t.away;
                //foreach (string user in usersToAdd)
                //    row[user] = DidUserMakeThisPick(t.home, t.week, user) ? t.home : DidUserMakeThisPick(t.away, t.week, user) ? t.away : "NONE"; // there is a bug here if they made no picks.
                row["Winner"] = t.winner ?? "??";

                if (showLive)
                    row["Now"] = t.live_update ?? string.Empty;

                matchups.Rows.Add(row);
            }
            return matchups;
        }
        private static System.Data.DataTable ConstructMatchupsDT(string[] usersToAdd)
        {
            System.Data.DataTable matchups = new System.Data.DataTable();
            matchups.Columns.Add("Date");
            matchups.Columns.Add("Channel");
            matchups.Columns.Add("Visitor");
            matchups.Columns.Add("VisitorScore");
            matchups.Columns.Add("Home");
            matchups.Columns.Add("HomeScore");
            matchups.Columns.Add("Status");
            //matchups.Columns.Add("YourPick");
            //foreach (string user in usersToAdd)
            //    matchups.Columns.Add(user);
            matchups.Columns.Add("Winner");
            return matchups;
        }
        private static bool DidUserMakeThisPick(string team, int week, string username = null)
        {
            try
            {
                username = username ?? Authentication.GetAuthenticatedUserName();
                return Common.DBModel().NFL_userPicks.First(x => x.username == username && x.week == week).picks.Contains(team);
            }
            catch { return false; }
        }
        private static string DateFromEID(long? eid)
        {
            string result = string.Empty;
            if (eid.HasValue)
            {
                string dateString = eid.Value.ToString();
                result = dateString.Substring(4, 2) + "/" + dateString.Substring(6, 2) + "/" + dateString.Substring(0, 4);
            }
            return result;
        }
        #endregion
    }
}
