using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Reflection;
using System.Collections;
using System.Web.SessionState;

namespace HomeWebApp
{
    public class Common : HomeAppsLib.LibCommon
    {
        public static string ALBUM_ROOT_PHYSICAL_DIR = AppSetting("ALBUM_ROOT_PHYSICAL_DIR");
        public static string ALBUM_ROOT_PHYSICAL_DIR_SMALL = AppSetting("ALBUM_ROOT_PHYSICAL_DIR_SMALL");
        public static string ALBUM_ROOT_VIRTUAL_DIR_SMALL = AppSetting("ALBUM_ROOT_VIRTUAL_DIR_SMALL");
        public static string ALBUM_ROOT_VIRTUAL_DIR = AppSetting("ALBUM_ROOT_VIRTUAL_DIR");
        public static string VIDEO_ROOT_PHYSICAL_DIR = AppSetting("VIDEO_ROOT_PHYSICAL_DIR");
        public static string EMAIL_SUB_SETTING_KEY_PREFIX = "SubscribedToEmail-";

        public static HomeAppsLib.db.user CurrentUser
        {
            get
            {
                string username = Authentication.GetAuthenticatedUserName();
                return HomeAppsLib.LibCommon.DBModel().users.FirstOrDefault(x => x.name == username);
            }
        }
        public static bool UserHasEmail
        {
            get
            {
                var user = CurrentUser;
                return user != null && user.email != null;
            }
        }
        public static bool UserHasEmailSub
        {
            get
            {
                string username = CurrentUser.name;
                return UserHasEmail && DBModel().userSettings.Count(x => x.active_flag && x.settingKey.Contains(EMAIL_SUB_SETTING_KEY_PREFIX) && x.username == username) > 0;
            }
        }
        public static bool RapidFireMode
        {
            get
            {
                return Common.IsAppInMode("NFL_PICKS_MOBILE_RAPID_FIRE");
            }
        }

        public static bool AutoLoadAdminPanel        
        {
            get
            {
                if (HttpContext.Current.Session["AutoLoadAdminPanel"] == null)
                    return false;
                else return Convert.ToBoolean(HttpContext.Current.Session["AutoLoadAdminPanel"]);
            }
            set 
            {
                HttpContext.Current.Session["AutoLoadAdminPanel"] = value;
            }
        }

        public static string[] GetPictureFiles(string dir)
        {
            return GetFiles(dir, PictureFileExtensions());
        }

        private static string[] GetFiles(string dir, string[] searchPatterns)
        {
            List<string> files = new List<string>();

            foreach (string searchPattern in searchPatterns)
                foreach (string file in System.IO.Directory.GetFiles(dir, searchPattern))
                    files.Add(file);

            return files.ToArray();
        }

        private static string[] PictureFileExtensions()
        {
            return new string[] { "*.jpg", "*.png", "*.bmp" };
        }

        private static string AppSetting(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        public static string SendEmail(string to, string subject, string body, string displayName)
        {
            return HomeAppsLib.LibCommon.SendEmail(to, subject, body, displayName);
        }        

        internal static string DBConnectionString()
        {
            //bool dev = Environment.MachineName.ToLower() == "sf-11789";
            //string conStrKey = dev ? "HomeWebAppDBConnectionStringDev" : "HomeWebAppDBConnectionStringProd";

            return System.Configuration.ConfigurationManager.ConnectionStrings["HomeWebAppDBConnectionString"].ConnectionString;
        }

        internal static db.DBModelDataContext DBModel()
        {
            db.DBModelDataContext context = new db.DBModelDataContext(DBConnectionString());
            return context;
        }

        public static List<string[]> GetMatchups(int week)
        {
            List<string[]> result = new List<string[]>();

            foreach (db.NFL_Matchup matchup in DBModel().NFL_Matchups.Where(x => x.week == week))
            {
                string[] temp = new string[2];
                temp[0] = matchup.away;
                temp[1] = matchup.home;

                result.Add(temp);
            }

            return result;
        }

        internal static void AddVisitorCustomMessage(string ip, string message)
        {
            var data = DBModel();
            var vis = data.visitors.First(x => x.ip_address == ip);
            vis.message = message;
            data.SubmitChanges();

        }

        internal static bool UserHasCustomMessage(string ip)
        {
            var data = DBModel();
            if (data.visitors.Count(x => x.ip_address == ip) == 0) return false;
            else return data.visitors.First(x => x.ip_address == ip).message != null;
        }

        internal static string GetUserCustomMessage(string ip)
        {
            try
            {
                return DBModel().visitors.First(x => x.ip_address == ip).message;
            }
            catch (Exception ex) { return ex.Message; }
        }

        internal static void UpdateUserCustomMessage(string ip, string message)
        {
            var data = DBModel();

            var vis = data.visitors.First(x => x.ip_address == ip);

            db.visitorCustomMessageHistory visHist = new db.visitorCustomMessageHistory();
            visHist.ip_address = ip;
            visHist.message = vis.message;
            visHist.update_dt = DateTime.Now;
            data.visitorCustomMessageHistories.InsertOnSubmit(visHist);

            vis.message = message;

            data.SubmitChanges();
        }

        public static string GetFirstFileByAlbum(string album)
        {
            string[] findHeader = System.IO.Directory.GetFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + album, "*.header");
            if (findHeader.Length > 0) return System.IO.Path.GetFileName(findHeader[0]).Replace(".header", "");

            string[] files = System.IO.Directory.GetFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + album);
            return files.Length == 0 ? string.Empty : System.IO.Path.GetFileName(files[0]);
        }

        public static void SetPicAlbumCover(string album, string picture)
        {
            foreach (string file in System.IO.Directory.GetFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + album, "*.header"))
                System.IO.File.Delete(file);

            string picPath = Common.ALBUM_ROOT_PHYSICAL_DIR + album + "\\" + picture;
            System.IO.File.Copy(picPath, picPath + ".header");
        }

        internal static int FindGridViewColumnIndexByHeader(System.Web.UI.WebControls.GridView gv, string header)
        {
            for (int index = 0; index < gv.Columns.Count; index++)
            {
                if (gv.Columns[index].HeaderText == header) return index;
            }

            throw new Exception("Exception in Common.FindGridViewColumnIndexByHeader, unable to find header {" + header + "} in grid view {" + gv.ID + "}");
        }

        internal static void CheckForUserToVisitorInsert(string username, string ipAddress)
        {
            if (username != null)
            {
                if (DBModel().usersToVisitors.Count(x => x.username == username && x.ip_address == ipAddress) == 0)
                {
                    var data = DBModel();
                    db.usersToVisitor uToV = new db.usersToVisitor();

                    uToV.username = username;
                    uToV.ip_address = ipAddress;
                    uToV.insert_dt = DateTime.Now;

                    data.usersToVisitors.InsertOnSubmit(uToV);
                    data.SubmitChanges();
                }
            }
        }

        internal static bool HasUserMadePicksForWeek(int week)
        {
            return DBModel().NFL_userPicks.Count(x => x.week == week && x.username == Authentication.GetAuthenticatedUserName()) > 0;
        }

        internal static void AddNewNFLForumComment(int week, string username, int? refId, string comment, string title, bool emailOnReply)
        {
            if (CheckForDuplicate(comment))
                return;

            var data = HomeAppsLib.LibCommon.DBModel();
            HomeAppsLib.db.NFL_forum post = new HomeAppsLib.db.NFL_forum();

            post.week = week;
            post.ref_id = refId;
            post.comment = comment;
            post.title = title;
            post.insert_dt = DateTime.Now;
            post.username = username;
            post.alerted = false;
            post.emailOnReply = emailOnReply;

            data.NFL_forums.InsertOnSubmit(post);
            data.SubmitChanges();

            if (refId.HasValue)
            {
                var parentPost = HomeAppsLib.LibCommon.DBModel().NFL_forums.FirstOrDefault(x => x.id == refId.Value);
                if (parentPost != null && parentPost.emailOnReply && parentPost.username != username)
                {
                    SendPostReplyEmail(post, parentPost.username);
                }
            }
        }

        private static bool CheckForDuplicate(string comment)
        {
            DateTime since = DateTime.Now.AddMinutes(-15);
            HomeAppsLib.db.NFL_forum dupe = HomeAppsLib.LibCommon.DBModel().NFL_forums.FirstOrDefault(x => x.comment == comment && x.insert_dt > since);
            return dupe != null;
        }

        private static void SendPostReplyEmail(HomeAppsLib.db.NFL_forum post, string usernameToSendEmail)
        {
            string subject = CurrentUser.name + " just replied to your post!";
            string to = DBModel().users.First(x => x.name == usernameToSendEmail).email;
            string body = HomeAppsLib.EmailSubscriptions.GenerateDiscPostEmailBodyText(post);

            HomeAppsLib.LibCommon.SendSystemEmailWithSignature(to, subject, body);
        }

        internal static string FindTopicTitle(int id)
        {
            var comment = HomeAppsLib.LibCommon.DBModel().NFL_forums.First(x => x.id == id);

            if (comment.title == null) return FindTopicTitle((int)comment.ref_id);
            else return comment.title;
        }

        internal static System.Web.UI.Control FindWhatControlDidPostBack(System.Web.UI.Page page)
        {
            System.Web.UI.Control result;

            if (page.IsPostBack)
            {
                string eventTarget = page.Request.Params["__EVENTTARGET"];

                if (!string.IsNullOrEmpty(eventTarget))
                {
                    result = page.FindControl(eventTarget);
                    if (result != null) return result;
                }

                foreach (var ctlID in page.Request.Form.AllKeys)
                {

                    System.Web.UI.Control c = page.FindControl(ctlID) as System.Web.UI.Control;

                    if (c is System.Web.UI.WebControls.Button)
                    {

                        return c;

                    }

                }

                System.Web.UI.Control ctrl = new System.Web.UI.Control();
                ctrl.ID = "noControlFound";
                return ctrl;
            }

            else
            {
                System.Web.UI.Control ctrl = new System.Web.UI.Control();
                ctrl.ID = "notPostBack";
                return ctrl; 
            }

        }

        internal static string GetDiscussionParticipantsById(int id)
        {
            List<string> participants = new List<string>();
            var row = HomeAppsLib.LibCommon.DBModel().NFL_forums.First(x => x.id == id);

            participants.Add(row.username);

            foreach (string user in GetAllRepliersRecursively(id))
                if (!participants.Contains(user)) participants.Add(user);

            return string.Join(",", participants.ToArray());
        }

        private static string[] GetAllRepliersRecursively(int id)
        {
            List<string> names = new List<string>();
            foreach (var row in HomeAppsLib.LibCommon.DBModel().NFL_forums.Where(x => x.ref_id == id))
            {
                names.Add(row.username);
                foreach (string name in GetAllRepliersRecursively(row.id))
                    names.Add(name);
            }

            return names.ToArray();

        }

        internal static int GetTopicId(int? refId)
        {
            if (refId == null) return LatestTopicId();
            else
            {
                var data = HomeAppsLib.LibCommon.DBModel();
                var comment = data.NFL_forums.First(x => x.id == refId);
                if (comment.ref_id == null) return comment.id;
                else return GetTopicId(comment.ref_id.Value);
            }
        }

        internal static void SetStayLoggedOnValue(DateTime stayLoggedOn)
        {
            string username = Authentication.GetAuthenticatedUserName();

            var data = DBModel();
            var user = data.users.First(x => x.name == username);
            user.stay_logged_on_dt = stayLoggedOn;

            data.SubmitChanges();
        }

        internal static void SetCummulativeStatsGrouping(int groupBy)
        {
            SetUserSetting(CummulativeStatsGroupingSettingKey(), groupBy.ToString());
        }

        internal static string GetCummalitveStatsGrouping()
        {
            return GetUserSetting(CummulativeStatsGroupingSettingKey(), "50");
        }

        internal static void SetLastVisitedWeek(int week)
        {
            string key = LastNFLWeekVisitedSettingKey();
            SetUserSetting(key, week.ToString());            
        }

        internal static string GetLastWeekVisited()
        {
            string key = LastNFLWeekVisitedSettingKey();
            return GetUserSetting(key, "-1");
        }

        public static void SetUserSetting(string key, string value)
        {
            AddUserSetting(key, value, true); 
        }

        private static void AddUserSetting(string key, string value, bool distinct)
        {
            string username = Authentication.GetAuthenticatedUserName();
            if (string.IsNullOrEmpty(username))
                return;

            var data = DBModel();

            if (distinct)
            {
                foreach (var item in data.userSettings.Where(x => x.settingKey == key && x.active_flag == true && x.username == username))
                    item.active_flag = false;
            }

            db.userSetting newSetting = new db.userSetting();
            newSetting.username = username;
            newSetting.settingKey = key;
            newSetting.settingValue = value;
            newSetting.insert_dt = DateTime.Now;
            newSetting.update_dt = DateTime.Now;
            newSetting.active_flag = true;

            data.userSettings.InsertOnSubmit(newSetting);

            data.SubmitChanges(); 
        }

        public static string GetUserSetting(string key, string defaultValue)
        {
            string username = Authentication.GetAuthenticatedUserName();
            var data = DBModel();

            var settings = data.userSettings.Where(x => x.settingKey == key && x.active_flag == true && x.username == username);

            if (settings.Count() > 0)
                return settings.First().settingValue;
            else return defaultValue; 
        }
        public static void DeleteUserSetting(string key, bool softDelete = false)
        {
            string username = Authentication.GetAuthenticatedUserName();
            var data = DBModel();

            var settings = data.userSettings.Where(x => x.settingKey == key && x.username == username);
            if (softDelete)
                settings = settings.Where(x => x.active_flag);

            foreach (var setting in settings)
            {
                if (softDelete)
                    setting.active_flag = false;
                else
                    data.userSettings.DeleteOnSubmit(setting);
            }
            data.SubmitChanges();
        }

        private static string LastNFLWeekVisitedSettingKey()
        {
            return "last_nfl_week_visited"; 
        }

        private static string CummulativeStatsGroupingSettingKey()
        {
            return "cummulative_stats_grouping";
        }

        private static int LatestTopicId()
        {
            return HomeAppsLib.LibCommon.DBModel().NFL_forums.OrderByDescending(x => x.insert_dt).First().id;
            
        }

        internal static int GetTotalNumberOfNFLMatchups()
        {
            return DBModel().NFL_Matchups.Count();
        }

        public static bool UserHasNotSubmittedComment()
        {
            try
            {
                string currentUser = Authentication.GetAuthenticatedUserName();

                if (IsAppInTestMode() && currentUser == "Eric")
                {
                    return true;
                }

                else
                    return HomeAppsLib.LibCommon.DBModel().NFL_forums.Count(x => x.username == currentUser) == 0 && Authentication.IsUserAuthenticated();
            }
            catch { return false; }
        }        

        internal static bool ArePicksClosedForWeek(int week)
        {
            return DBModel().NFL_weeks.First(x => x.week == week).picksClosed && week > 0;
        }

        internal static int GetDefaultWeek(int? sportId = null, bool toMakePicks = false)
        {
            if (!sportId.HasValue)
                sportId = (int)HomeAppsLib.Sports.NFL;

            var resultWeek = DBModel().NFL_weeks.Where(x => x.picksClosed == false && x.week > 0 && x.exp_dt > DateTime.Now);
            if (resultWeek != null && resultWeek.Count() > 0)
            {
                return resultWeek.Min(x => x.week);
            }
            else
            {
                int yearId = DBModel().NFL_years.Where(x => x.sport_id == sportId).Max(x => x.id);

                string username = Authentication.GetAuthenticatedUserName();
                var candidateWeeks = DBModel().NFL_weeks.Where(x => x.year_id == yearId);
                if (!string.IsNullOrEmpty(username))
                {
                    int[] pickedWeekIds = DBModel().NFL_userPicks.Where(x => x.username == username).Select(x => x.week).ToArray();
                    if (toMakePicks && !Common.CurrentUser.IsKid)
                        return candidateWeeks.Where(x => x.exp_dt > DateTime.Now && !pickedWeekIds.Contains(x.week)).Min(x => x.week);
                    else if (toMakePicks)
                        return candidateWeeks.Where(x => !pickedWeekIds.Contains(x.week)).Min(x => x.week);
                    else
                        return candidateWeeks.Where(x => !x.picksClosed).Min(x => x.week);
                }
                else
                {
                    var unexpiredCandidates = candidateWeeks.Where(x => x.exp_dt > DateTime.Now);
                    var openCandidates = candidateWeeks.Where(x => !x.picksClosed);

                    if (toMakePicks && unexpiredCandidates.Count() > 0)
                        return unexpiredCandidates.Min(x => x.week);
                    else if (openCandidates.Count() > 0)
                        return openCandidates.Min(x => x.week);
                    else
                        return candidateWeeks.Min(x => x.week);
 
                }
            }
        }

        internal static void SubmitUserPicks(int weekInt, string[] picks)
        {
            if (!Authentication.IsUserAuthenticated())
                return;
            
            var data = DBModel();
            db.NFL_userPick pick = new db.NFL_userPick();
            pick.username = Authentication.GetAuthenticatedUserName();
            pick.picks = string.Join(",", picks);
            pick.pick_dt = DateTime.Now;
            pick.week = weekInt;
            data.NFL_userPicks.InsertOnSubmit(pick);
            data.SubmitChanges();
        }

        internal static void PublishNflWeek(int selectedWeek)
        {
            var data = DBModel();
            var week = data.NFL_weeks.First(x => x.week == selectedWeek);
            week.display_flag = true;
            data.SubmitChanges();
        }

        internal static void DeleteAllMatchups(int selectedWeek)
        {
            var data = DBModel();
            var week = data.NFL_weeks.First(x => x.week == selectedWeek);
            week.display_flag = false;

            var matchups = data.NFL_Matchups.Where(x => x.week == selectedWeek);
            data.NFL_Matchups.DeleteAllOnSubmit(matchups);

            data.SubmitChanges();
        }

        internal static DateTime MaxReplyDate(HomeAppsLib.db.NFL_forum post)
        {
            DateTime result = DateTime.MinValue;

            foreach (var reply in HomeAppsLib.LibCommon.DBModel().NFL_forums.Where(x => x.ref_id == post.id))
            {
                if (reply.insert_dt.HasValue && reply.insert_dt.Value > result)
                    result = reply.insert_dt.Value;

                DateTime maxSubReply = MaxReplyDate(reply);
                if (maxSubReply > result)
                    result = maxSubReply;
            }

            return result;
        }

        public static List<string> GetAllLoggedInUsers()
        {
            List<String> activeSessions = new List<String>();
            object obj = typeof(HttpRuntime).GetProperty("CacheInternal", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null);
            object[] obj2 = (object[])obj.GetType().GetField("_caches", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
            for (int i = 0; i < obj2.Length; i++)
            {
                Hashtable c2 = (Hashtable)obj2[i].GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj2[i]);
                foreach (DictionaryEntry entry in c2)
                {
                    object o1 = entry.Value.GetType().GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(entry.Value, null);
                    if (o1.GetType().ToString() == "System.Web.SessionState.InProcSessionState")
                    {
                        SessionStateItemCollection sess = (SessionStateItemCollection)o1.GetType().GetField("_sessionItems", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o1);
                        if (sess != null)
                        {
                            if (sess[Authentication.AUTHENTICATED_USER_STRING] != null)
                            {
                                activeSessions.Add(sess[Authentication.AUTHENTICATED_USER_STRING].ToString());
                            }
                        }
                    }
                }
            }
            return activeSessions;
        }

        internal static int? GetNextWeek(int weekInt)
        {
            var lastWeek = Common.DBModel().NFL_weeks.First(x => x.week == weekInt);
            var yearWeeks = Common.DBModel().NFL_weeks.Where(x => x.year_id == lastWeek.year_id);

            if (yearWeeks.Max(x => x.year_week) > lastWeek.year_week)
                return yearWeeks.FirstOrDefault(x => x.year_week == lastWeek.year_week + 1).week;
            else
                return null;            
        }

        internal static int GetDefaultNFLYearId()
        {
            return Common.DBModel().NFL_years.Where(y => y.sport_id == 100).Max(y => y.id);
        }

        internal static int GetBetUnitAmount(int yearId)
        {
            return Common.DBModel().NFL_years.First(y => y.id == yearId).betAmount.GetValueOrDefault();
        }

        public static int GetCurrentSportId()
        {
            if (HttpContext.Current.Session["CurrentSportId"] == null)
                SetCurrentSportId(100);

            return Convert.ToInt32(HttpContext.Current.Session["CurrentSportId"]);
        }
        public static void SetCurrentSportId(int id)
        {
            HttpContext.Current.Session["CurrentSportId"] = id;
            HttpContext.Current.Session["CurrentSportDisplay"] = Common.DBModel().NFL_sports.First(x => x.id == id).name;
        }
        public static string[] getUsersToAddToMatchupsGrid()
        {
            string[] result = null;
            if (HttpContext.Current.Session["usersToAddToMatchupsGrid"] == null)
            {
                string usersPipeDelimted = Common.GetUserSetting("usersToAddToMatchupsGrid", "");
                if (!string.IsNullOrEmpty(usersPipeDelimted))
                {
                    string[] users = usersPipeDelimted.Split('|');
                    HttpContext.Current.Session["usersToAddToMatchupsGrid"] = users;
                }
            }
            if (HttpContext.Current.Session["usersToAddToMatchupsGrid"] != null)
            {
                result = (string[])HttpContext.Current.Session["usersToAddToMatchupsGrid"];
            }

            return result;
        }
        public static void setUsersToAddToMatchupsGrid(string[] users)
        {
            if (users == null)
            {
                Common.DeleteUserSetting("usersToAddToMatchupsGrid");
                HttpContext.Current.Session["usersToAddToMatchupsGrid"] = null;
            }
            else
            {
                Common.SetUserSetting("usersToAddToMatchupsGrid", string.Join("|", users));
                HttpContext.Current.Session["usersToAddToMatchupsGrid"] = users;
            }

        }
    }


}