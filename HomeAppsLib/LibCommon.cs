using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeAppsLib.API;

namespace HomeAppsLib
{
    public class TimeLogItem
    {
        public string Desc { get; set; }
        public DateTime TimeStamp { get; set; }
        public TimeSpan Duration { get; set; }
    }
    public class TimeLog
    {
        public List<TimeLogItem> Items { get; set; }

        public TimeLog()
        {
            Items = new List<TimeLogItem>();
            Items.Add(new TimeLogItem { Desc = "init", TimeStamp = DateTime.Now, Duration = TimeSpan.Zero });
        }

        public void Add(string desc)
        {
            TimeLogItem item = new TimeLogItem();
            item.Desc = desc;
            item.TimeStamp = DateTime.Now;
            item.Duration = item.TimeStamp - Items.Last().TimeStamp;

            Items.Add(item);
        }

        public void Print(string fileName)
        {
            string print = string.Join("\r\n\r\n", Items.OrderByDescending(i => i.Duration).Select(i => i.Desc + "||" + i.TimeStamp.ToString() + "||" + i.Duration.Seconds).ToArray());
            System.IO.File.WriteAllText(@"c:\temp\" + fileName + ".txt", print);
        }
    }

    public class LibCommon
    {
        public static db.dataDataContext DBModel()
        {
            return new db.dataDataContext(DBConnectionString());
        }

        private static string DBConnectionString()
        {
            //bool dev = Environment.MachineName.ToLower() == "sf-11789";
            //string conStrKey = dev ? "HomeWebAppDBConnectionStringDev" : "HomeWebAppDBConnectionStringProd";

            return System.Configuration.ConfigurationManager.ConnectionStrings["HomeWebAppDBConnectionString"].ConnectionString;
        }

        public static string SendEmail(string to, string subject, string body, string displayName, TimeSpan? waitAfter = null)
        {
            return SendEmail(to, subject, body, displayName, true, waitAfter: waitAfter);
        }

        private static string SendEmail(string to, string subject, string body, string displayName, bool logEmail, TimeSpan? waitAfter = null)
        {
            if (string.IsNullOrEmpty(to))
                return "parameter 'to' was sent to method 'SendEmail' as an empty or null string";

            // return "Email Service is Down";
            bool success;
            string message;

            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient("smtp.gmail.com");
                client.EnableSsl = true;
                client.Credentials = GetMyCredentials();
                client.Port = 587;

                mail.From = new System.Net.Mail.MailAddress(GetMyCredentials().UserName, displayName);

                string[] recipients = to.Split(';');
                foreach (string rec in recipients)
                {
                    if (rec.Trim() != string.Empty) mail.To.Add(rec.Trim());
                }                
                
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                client.Send(mail);

                if (waitAfter.HasValue)
                {
                    System.Threading.Thread.Sleep((int)waitAfter.Value.TotalMilliseconds);
                }

                success = true;
                message = "Success";
            }

            catch (Exception ex)
            {
                success = false;
                message = ex.ToString();
            }

            
            // log email
            if (logEmail)
            {
                try
                {
                    var data = DBModel();
                    var log = new db.EmailLog();
                    log.EmailTo = to;
                    log.EmailSubject = subject;
                    log.EmailBody = body;
                    log.EmailDisplayFrom = displayName;
                    log.Success = success;
                    log.Message = message;
                    log.SendDate = DateTime.Now;
                    try { log.Source = System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name; }
                    catch (Exception ex) { log.Source = "ERROR: " + ex.Message; }
                    data.EmailLogs.InsertOnSubmit(log);
                    data.SubmitChanges();
                }
                catch (Exception ex) { SendEmail("eric@hackerdevs.com", "ERROR LOGGING EMAIL", ex.ToString(), "Home Apps", false); }
            }

            return message;
        }

        public static string SendCoryText(int week)
        {
            //href = '" + LibCommon.WebsiteUrlRoot() + "nflpicks.aspx?quickpicks=true&autoweek=" + weekAboutToExpire.week + "&sso=" + LibCommon.SSOUserKey(user) + "'
            string url = "http://www.thewrightpicks.com/nflpicks.aspx?quickpicks=true&autoweek=" + week + "&sso=7E1191C4537CC9575D2E4D6AB70608C6849C2326836A339D";

            // sana
            // AE31CA5C7433D36CDB3654C2F76F7634A8CC4298A376EF5F
            // cory
            // 7E1191C4537CC9575D2E4D6AB70608C6849C2326836A339D

            return SendText("8328923624", "Make your picks! Click here: " + url);
            //return SendText("7138051398", "Make your picks! Click here: " + url);
            //return SendText("8325676572", "Make your picks! Click here: " + url);
        }
        public static string SendJCText(int week)
        {
            //href = '" + LibCommon.WebsiteUrlRoot() + "nflpicks.aspx?quickpicks=true&autoweek=" + weekAboutToExpire.week + "&sso=" + LibCommon.SSOUserKey(user) + "'
            string url = "http://www.thewrightpicks.com/nflpicks.aspx?quickpicks=true&autoweek=" + week + "&sso=8C6E63886CD5DFFFEE916C012753C42016140864289A17B9CF151776DFFA5D25";
            //string message = "<a href='" + url + "'>CLICK HERE YOU MAKE YOUR PICKS!</a>";
            string message = "[url=" + url + "]CLICK HERE YOU MAKE YOUR PICKS![/url]";

            //return SendText("7134580859", "Make your picks! Click here: " + url, domain: "tmomail.net");
            return SendText("7138051398", message);
            //return SendText("8325676572", "Make your picks! Click here: " + url);
        }

        public static string SendText(string number, string message, string domain = "pm.sprint.com")
        {
            // href='" + LibCommon.WebsiteUrlRoot() + "nflpicks.aspx?quickpicks=true&autoweek=" + weekAboutToExpire.week + "&sso=" + LibCommon.SSOUserKey(user) + "'
            string to = number + "@" + domain;
            return SendEmail(to, string.Empty, message, "The Wright Picks");
        }

        private static System.Net.NetworkCredential GetMyCredentials()
        {
            System.Net.NetworkCredential me = new System.Net.NetworkCredential();
            me.UserName = "thewrightpicks@gmail.com";
            me.Password = "DDEBB0F1-CAB5-4785-B069-476139015692";
            //me.UserName = "admin@thewrightpicks.com";
            //me.Password = "threwwu7";
            return me;
        }

        public static void ChangeAppMode(bool modeOn, string modeText = "testmode")
        {
            var data = DBModel();

            if (modeOn)
            {
                db.userSetting setting = new db.userSetting();
                setting.active_flag = true;
                setting.settingKey = modeText;
                setting.settingValue = modeText;
                setting.update_dt = DateTime.Now;
                setting.insert_dt = DateTime.Now;
                setting.username = "Eric";

                data.userSettings.InsertOnSubmit(setting);
                data.SubmitChanges();
            }

            else
            {
                data.userSettings.DeleteAllOnSubmit(data.userSettings.Where(x => x.settingKey == modeText));
                data.SubmitChanges();
            }
        }

        public static bool IsAppInTestMode()
        {
            return IsAppInMode("testmode");
        }

        public static bool IsAppInMode(string modeText)
        {
            return DBModel().userSettings.Count(x => x.settingKey == modeText) > 0;
        }

        public static string GetUserNameFromEncryptedPassword(string userKey)
        {
            var user = DBModel().users.FirstOrDefault(x => x.encPW == userKey);
            if (user != null)
                return user.name;
            else return null;
        }

        public static string WebsiteUrlRoot(/*bool local = false*/)
        {
            //if (LibCommon.IsDevelopmentEnvironment())
               // return "http://localhost:60002/";

            //return local ? "http://192.168.2.150/" : "http://www.thewrightpicks.com/";            

            return "http://www.thewrightpicks.com/";
        }

        public static bool IsDevelopmentEnvironment()
        {
            return Environment.MachineName.ToLower() == "sf-11789";
        }

        public static string SendSystemEmailWithSignature(string to, string subject, string body)
        {
            body += "<br><a href='" + WebsiteUrlRoot() + "'>Click here to visit The Wright Picks</a><br><br>Thanks,<br>The Wright Picks";
            return SendEmail(to, subject, body, "The Wright Picks");
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // valid email
            // 1- contains exactly 1 @
            // 2- contains .
            // 3- does NOT contain ..

            if (!email.Contains("@") || email.StartsWith("@") || email.EndsWith("@") || email.Count(x => x == '@') != 1)
                return false;
            if (!email.Contains(".") || email.StartsWith(".") || email.EndsWith(".") || email.Contains(".."))
                return false;

            return true;
        }

        private static bool _changesMade;
        public static bool UpdateNFLMatchups(bool async)
        {
            // note: async true will always return changeMade false. if calling async should disregard response.
            _changesMade = false;
            LibCommon m = new LibCommon();
            System.Threading.Thread t = new System.Threading.Thread(m.DoUpdateNFLMatchupsESPN);
            t.Start();
            if (!async)
            {
                while (t.IsAlive) System.Threading.Thread.Sleep(1);
            }
            return _changesMade;
        }

        private void DoUpdateNFLMatchups()
        {
            try
            {
                var data = DBModel();
                int[] nflYears = data.NFL_years.Where(x => x.sport_id == 100).Select(x => x.id).ToArray();
                int[] nflWeeks = data.NFL_weeks.Where(x => nflYears.Contains(x.year_id.Value)).Select(x => x.week).ToArray();

                var matchupsWithoutWinner = data.NFL_Matchups.Where(x => x.winner == null && nflWeeks.Contains(x.week));
                bool changesMade = false;

                System.Net.WebClient wc = new System.Net.WebClient();
                System.Xml.XmlDocument xDocRegSeason = new System.Xml.XmlDocument();
                xDocRegSeason.LoadXml(wc.DownloadString("http://www.nfl.com/liveupdate/scorestrip/ss.xml"));
                System.Xml.XmlDocument xDocPostSeason = new System.Xml.XmlDocument();
                xDocPostSeason.LoadXml(wc.DownloadString("http://www.nfl.com/liveupdate/scorestrip/postseason/ss.xml"));

                foreach (var matchup in matchupsWithoutWinner)
                {
                    int week = matchup.year_week ?? matchup.week;
                    int yearId = data.NFL_weeks.First(x => x.week == matchup.week).year_id ?? 1;
                    int year = data.NFL_years.First(x => x.id == yearId).year;

                    System.Xml.XmlNode node = null;

                    // regular season
                    if (matchup.NFL_week.weekTypeId == (int)WeekTypes.RegularSeason)
                    {
                        node = xDocRegSeason.SelectSingleNode("//ss/gms[@w='" + week + "' and @y='" + year + "' and @t='R']/g[@hnn='" + matchup.home.ToLower() + "' and @vnn='" + matchup.away.ToLower() + "']");
                        if (node != null)
                        {
                            UpdateMatchup(matchup, node);
                            changesMade = true;
                        }
                    }

                    // pre season
                    if (matchup.NFL_week.weekTypeId == (int)WeekTypes.PreSeason)
                    {
                        node = xDocRegSeason.SelectSingleNode("//ss/gms[@w='" + week + "' and @y='" + year + "' and @t='P']/g[@hnn='" + matchup.home.ToLower() + "' and @vnn='" + matchup.away.ToLower() + "']");
                        if (node != null)
                        {
                            UpdateMatchup(matchup, node);
                            changesMade = true;
                        }
                    }

                    // post season (playoffs and super bowl)
                    if (matchup.NFL_week.weekTypeId == (int)WeekTypes.PostSeason)
                    {
                        node = xDocPostSeason.SelectSingleNode("//ss/gms[@y='" + year + "' and (@t='POST' or @t='PRO')]/g[@hnn='" + matchup.home.ToLower() + "' and @vnn='" + matchup.away.ToLower() + "']");
                        if (node != null)
                        {
                            UpdateMatchup(matchup, node);
                            changesMade = true;
                        }
                    }


                    //// Pro Bowl
                    //if (matchup.home == "AFC" || matchup.home == "NFC")
                    //{
                    //    node = xDocPostSeason.SelectSingleNode("//ss/gms[@y='" + year + "' and @t='POST']/g[@hnn = 'afc pro bowl' or @vnn = 'afc pro bowl']");
                    //    if (node != null)
                    //    {
                    //        UpdateMatchup(matchup, node);
                    //        changesMade = true;
                    //    }

                    //}

                }

                if (changesMade)
                    data.SubmitChanges();

                checkIfWeeksNeedToBeClosed();


            }
            catch (Exception ex)
            {
                try
                {
                    //if (IsAppInTestMode())
                        LibCommon.SendEmail("eric@hackerdevs.com", "Exception in UpdateNFLMatchups()", ex.ToString(), "EricWrightSite"); 
                }
                catch { }
            }
        }

        private void DoUpdateNFLMatchupsJson()
        {
            try
            {
                System.Net.WebClient client = new System.Net.WebClient();
                API.NFL.NFL_API_Json model = new API.NFL.NFL_API_Json();
                string json = client.DownloadString("http://static.nfl.com/liveupdate/scores/scores.json");
                model.Matchups = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, API.NFL.Matchup>>(json);

                var data = DBModel();
                int[] nflYears = data.NFL_years.Where(x => x.sport_id == 100).Select(x => x.id).ToArray();
                int[] nflWeeks = data.NFL_weeks.Where(x => nflYears.Contains(x.year_id.Value)).Select(x => x.week).ToArray();

                var matchupsWithoutWinner = data.NFL_Matchups.Where(x => x.winner == null && nflWeeks.Contains(x.week)).ToList();
                bool changesMade = false;

                

                foreach (var matchup in matchupsWithoutWinner)
                {
                    int week = matchup.year_week ?? matchup.week;
                    int yearId = data.NFL_weeks.First(x => x.week == matchup.week).year_id ?? 1;
                    int year = data.NFL_years.First(x => x.id == yearId).year;

                    //System.Xml.XmlNode node = null;

                    KeyValuePair<string, API.NFL.Matchup> apiMatchup = new KeyValuePair<string, API.NFL.Matchup>();

                    // regular season
                    //if (matchup.NFL_week.weekTypeId == (int)WeekTypes.RegularSeason)
                    //{
                        apiMatchup = model.Matchups.FirstOrDefault(m => m.Value.away.abbr == GetAbbreviation(matchup.away) && m.Value.home.abbr == GetAbbreviation(matchup.home));
                        //node = xDocRegSeason.SelectSingleNode("//ss/gms[@w='" + week + "' and @y='" + year + "' and @t='R']/g[@hnn='" + matchup.home.ToLower() + "' and @vnn='" + matchup.away.ToLower() + "']");
                        if (apiMatchup.Key != null)
                        {
                            UpdateMatchup(matchup, apiMatchup);
                            changesMade = true;
                        }
                    //}

                    // pre season
                    //if (matchup.NFL_week.weekTypeId == (int)WeekTypes.PreSeason)
                    //{
                    //    node = xDocRegSeason.SelectSingleNode("//ss/gms[@w='" + week + "' and @y='" + year + "' and @t='P']/g[@hnn='" + matchup.home.ToLower() + "' and @vnn='" + matchup.away.ToLower() + "']");
                    //    if (node != null)
                    //    {
                    //        UpdateMatchup(matchup, node);
                    //        changesMade = true;
                    //    }
                    //}

                    // post season (playoffs and super bowl)
                    //if (matchup.NFL_week.weekTypeId == (int)WeekTypes.PostSeason)
                    //{
                    //    node = xDocPostSeason.SelectSingleNode("//ss/gms[@y='" + year + "' and (@t='POST' or @t='PRO')]/g[@hnn='" + matchup.home.ToLower() + "' and @vnn='" + matchup.away.ToLower() + "']");
                    //    if (node != null)
                    //    {
                    //        UpdateMatchup(matchup, node);
                    //        changesMade = true;
                    //    }
                    //}


                    //// Pro Bowl
                    //if (matchup.home == "AFC" || matchup.home == "NFC")
                    //{
                    //    node = xDocPostSeason.SelectSingleNode("//ss/gms[@y='" + year + "' and @t='POST']/g[@hnn = 'afc pro bowl' or @vnn = 'afc pro bowl']");
                    //    if (node != null)
                    //    {
                    //        UpdateMatchup(matchup, node);
                    //        changesMade = true;
                    //    }

                    //}

                }

                if (changesMade)
                    data.SubmitChanges();

                checkIfWeeksNeedToBeClosed();


            }
            catch (Exception ex)
            {
                try
                {
                    //if (IsAppInTestMode())
                        LibCommon.SendEmail("eric@hackerdevs.com", "Exception in DoUpdateNFLMatchupsJson()", ex.ToString(), "EricWrightSite");
                }
                catch { }
            }
        }

        private void DoUpdateNFLMatchupsESPN()
        {
            try
            {
                System.Net.WebClient client = new System.Net.WebClient();                
                string json = client.DownloadString("http://site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard");
                API.ESPN.Feed model = Newtonsoft.Json.JsonConvert.DeserializeObject<API.ESPN.Feed>(json);

                var data = DBModel();
                int[] nflYears = data.NFL_years.Where(x => x.sport_id == 100).Select(x => x.id).ToArray();
                int[] nflWeeks = data.NFL_weeks.Where(x => nflYears.Contains(x.year_id.Value)).Select(x => x.week).ToArray();

                var matchupsWithoutWinner = data.NFL_Matchups.Where(x => x.winner == null && nflWeeks.Contains(x.week) && model.week.number == x.year_week).ToList();                

                foreach (var matchup in matchupsWithoutWinner)
                {
                    //int week = matchup.year_week ?? matchup.week;
                   // int yearId = data.NFL_weeks.First(x => x.week == matchup.week).year_id ?? 1;
                    //int year = data.NFL_years.First(x => x.id == yearId).year;
                    
                    API.ESPN.Event apiMatchup = model.events.FirstOrDefault(m => 
                        CheckForWeirdName(m.competitions.First().competitors.First(c => c.homeAway == "home").team.shortDisplayName) == matchup.home &&
                        CheckForWeirdName(m.competitions.First().competitors.First(c => c.homeAway == "away").team.shortDisplayName) == matchup.away);                    
                    
                    if (apiMatchup != null)
                        UpdateMatchup(matchup, apiMatchup);
                }

                if (_changesMade)
                    data.SubmitChanges();

                checkIfWeeksNeedToBeClosed();


            }
            catch (Exception ex)
            {
                try
                {
                    //if (IsAppInTestMode())
                        LibCommon.SendEmail("eric@hackerdevs.com", "Exception in DoUpdateNFLMatchupsESPN()", ex.ToString(), "EricWrightSite");
                }
                catch { }
            }
        }

        private string CheckForWeirdName(string name)
        {
            if (name.ToLower() == "washington")
                return "Football Team";
            return name;
        }

        private string GetAbbreviation(string teamName)
        {
            if (TeamNamesToAbbriviations.Keys.Contains(teamName))
                return TeamNamesToAbbriviations[teamName];
            return string.Empty;
        }

        private Dictionary<string, string> TeamNamesToAbbriviations = new Dictionary<string, string> {
            { "Texans", "HOU" },
            { "Jaguars", "JAC" },
            { "Packers", "GB" },
            { "Vikings", "MIN" },
            { "Bears", "CHI" },
            { "Lions", "DET" },
            { "Colts", "IND" },
            { "Titans", "TEN" },
            { "Saints", "NO" },
            { "Falcons", "ATL" },
            { "Panthers", "CAR" },
            { "Buccaneers", "TB" },
            { "Giants", "NYG" },
            { "Cowboys", "DAL" },
            { "Football Team", "WAS" },
            { "Eagles", "PHI" },
            { "Cardinals", "ARI" },
            { "49ers", "SF" },
            { "Seahawks", "SEA" },
            { "Rams", "LA" },
            { "Browns", "CLE" },
            { "Bengals", "CIN" },
            { "Steelers", "PIT" },
            { "Ravens", "BAL" },
            { "Patriots", "NE" },
            { "Jets", "NYJ" },
            { "Dolphins", "MIA" },
            { "Bills", "BUF" },
            { "Broncos", "DEN" },
            { "Chargers", "LAC" },
            { "Chiefs", "KC" },
            { "Raiders", "LV" },
            { "NFC", "NFC" },
            { "AFC", "AFC" }
        };

        private void checkIfWeeksNeedToBeClosed()
        {
            var data = DBModel();
            var weeks = data.NFL_weeks;

            foreach (var week in weeks)
            {
                if (week.games == week.NFL_Matchups.Count)
                {
                    if (week.NFL_Matchups.Count(x => x.status == null || !x.status.StartsWith("F")) == 0 && week.NFL_Matchups.Count > 0)
                        week.picksClosed = true;
                }
            }

            data.SubmitChanges();

        }

        private void UpdateMatchup(db.NFL_Matchup matchup, System.Xml.XmlNode node)
        {
            matchup.status = node.Attributes["q"].Value;
            matchup.eid = Convert.ToInt64(node.Attributes["eid"].Value);
            matchup.scheduled = node.Attributes["d"].Value + " " + DateTime.Parse(node.Attributes["t"] == null ? "1:00 AM" : node.Attributes["t"].Value).AddHours(-1).ToShortTimeString().Split(' ')[0];

            if (matchup.status != "P")
            {
                int? oldHome = matchup.home_score;
                int? oldAway = matchup.away_score;

                matchup.home_score = Convert.ToInt32(node.Attributes["hs"].Value);
                matchup.away_score = Convert.ToInt32(node.Attributes["vs"].Value);

                if (matchup.home_score.HasValue && matchup.away_score.HasValue &&
                    (oldAway != matchup.away_score.Value || oldHome != matchup.home_score.Value))
                    _changesMade = true;
            }

            if (matchup.status == "F" || matchup.status == "FO")
            {
                string oldWinner = matchup.winner;

                if (matchup.home_score > matchup.away_score) matchup.winner = matchup.home;
                else if (matchup.away_score > matchup.home_score) matchup.winner = matchup.away;
                else matchup.winner = "TIE";

                if (!string.IsNullOrEmpty(matchup.winner) && oldWinner != matchup.winner)
                    _changesMade = true;
            }
        }

        private void UpdateMatchup(db.NFL_Matchup matchup, KeyValuePair<string, API.NFL.Matchup> node)
        {
            matchup.status = node.Value.qtr;
            matchup.eid = Convert.ToInt64(node.Key);
            //matchup.scheduled = node.Attributes["d"].Value + " " + DateTime.Parse(node.Attributes["t"] == null ? "1:00 AM" : node.Attributes["t"].Value).AddHours(-1).ToShortTimeString().Split(' ')[0];
            matchup.scheduled = node.Key;
            matchup.channel = node.Value.media.tv;            
            matchup.stadium = node.Value.stadium;

            int statusInt;
            bool live = int.TryParse(node.Value.qtr, out statusInt);
            matchup.live_update = live ?
                matchup.live_update = node.Value.posteam + "'s ball. " + NumberToPosition(node.Value.down) + " and " + node.Value.togo + " on " + node.Value.yl + ". Clock: " + node.Value.clock :
                null;

            if (live)
                _changesMade = true;

            if (matchup.status != null)
            {
                int? oldHome = matchup.home_score;
                int? oldAway = matchup.away_score;

                matchup.home_score = node.Value.home.score["T"];
                matchup.away_score = node.Value.away.score["T"];

                if (matchup.home_score.HasValue && matchup.away_score.HasValue &&
                    (oldAway != matchup.away_score.Value || oldHome != matchup.home_score.Value))
                    _changesMade = true;
            }

            if (matchup.status != null && matchup.status.ToLower().StartsWith("f"))
            {
                string oldWinner = matchup.winner;

                if (matchup.home_score > matchup.away_score) matchup.winner = matchup.home;
                else if (matchup.away_score > matchup.home_score) matchup.winner = matchup.away;
                else matchup.winner = "TIE";

                if (!string.IsNullOrEmpty(matchup.winner) && oldWinner != matchup.winner)
                    _changesMade = true;
            }
        }
        private void UpdateMatchup(db.NFL_Matchup matchup, API.ESPN.Event node)
        {
            matchup.status = node.status.type.state;
            matchup.eid = Convert.ToInt64(node.id);
            matchup.scheduled = node.date.ToLocalTime().ToShortDateString();

            List<string> channels = new List<string>();
            node.competitions.ToList().ForEach(c => c.broadcasts.ToList().ForEach(b => b.names.ToList().ForEach(n => channels.Add(n))));
            matchup.channel = string.Join(", ", channels.Distinct().ToArray());

            List<string> venues = new List<string>();
            node.competitions.ToList().ForEach(c => venues.Add(c.venue.fullName));
            matchup.stadium = string.Join(", ", venues.Distinct().ToArray());

            //int statusInt;
            bool live = node.status.type.state != "pre" && node.status.type.state != "post";
            matchup.live_update = live ?
                matchup.live_update = $"Clock: { node.status.displayClock }, Quarter: { node.status.period }" :
                null;

            if (live)
                _changesMade = true;

            if (matchup.status != null)
            {
                int? oldHome = matchup.home_score;
                int? oldAway = matchup.away_score;

                int homeScore;
                string homeScoreString = node.competitions.First().competitors.First(c => c.homeAway == "home").score;
                if (int.TryParse(homeScoreString, out homeScore))
                {
                    matchup.home_score = homeScore;
                }
                int awayScore;
                string awayScoreString = node.competitions.First().competitors.First(c => c.homeAway == "away").score;
                if (int.TryParse(awayScoreString, out awayScore))
                {
                    matchup.away_score = awayScore;
                }                

                if (matchup.home_score.HasValue && matchup.away_score.HasValue &&
                    (oldAway != matchup.away_score.Value || oldHome != matchup.home_score.Value))
                    _changesMade = true;
            }

            if (matchup.status != null && node.status.type.shortDetail.ToLower().StartsWith("final"))
            {
                string oldWinner = matchup.winner;

                matchup.status = node.status.type.shortDetail;

                if (matchup.home_score > matchup.away_score) matchup.winner = matchup.home;
                else if (matchup.away_score > matchup.home_score) matchup.winner = matchup.away;
                else matchup.winner = "TIE";

                if (!string.IsNullOrEmpty(matchup.winner) && oldWinner != matchup.winner)
                    _changesMade = true;
            }
        }

        private string NumberToPosition(string number)
        {
            int num;
            if (!int.TryParse(number, out num))
                return number;

            string suff;

            int ones = num % 10;
            int tens = (int)Math.Floor((decimal)num / 10) % 10;
            if (tens == 1) {
                suff = "th";
            }
            else {
                switch (ones) {
                    case 1 : suff = "st"; break;
                    case 2 : suff = "nd"; break;
                    case 3 : suff = "rd"; break;
                    default : suff = "th";break;
                }
            }
            return num + suff;
        }

        public static string SSOUserKey(db.user user)
        {
            Encryption enc = new Encryption();
            string pair = user.name + "|" + user.encPW;
            string key = enc.Encrypt(pair);
            return key;
        }
    }
}
