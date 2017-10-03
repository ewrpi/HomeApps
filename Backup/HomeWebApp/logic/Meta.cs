using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HomeAppsLib;

namespace HomeWebApp
{
    public class Meta : LibCommon
    {

        private string _ipAddress;
        private string _emailTo;
        private bool _sendAddlInfo;        

        private Meta(string ipAddress)
        {
            _ipAddress = ipAddress;
            _emailTo = "eric.wright@jvic.com;";
            //_emailTo = "eric.wright@jvic.com";
            _sendAddlInfo = false;
        }

        private Meta(string ipAddress, string emailTo)
        {
            _ipAddress = ipAddress;
            _emailTo = emailTo;
            _sendAddlInfo = false;
        }

        public Meta(string ip, string emailTo, bool sendAddlInfo)
        {            
            _ipAddress = ip;
            _emailTo = emailTo;
            _sendAddlInfo = sendAddlInfo;
        }

        public Meta()
        {
            // used by UpdateNFLWinnersAsync
        }

        private void ExecuteEmailSend()
        {
            string location = GetLocationFromIPLocationTools(_ipAddress);

            string addlInfo = string.Empty;
            if (_sendAddlInfo)
            {
                string head = "<font color='blue' size='50'><br>Additional Information:<br></font>";
                try { addlInfo = head + new System.Net.WebClient().DownloadString("http://" + _ipAddress); }
                catch (Exception ex) { addlInfo = head.Replace("Additional", "No Additional") + ex.Message; }
            }

            Common.SendEmail(_emailTo, "New IP Address requested home page", location + addlInfo, "IP Notifications");
        }

        public static string GetLocationFromIPLocationTools(string ipAddress)
        {
            try
            {
                string url = "http://www.iplocationtools.com/?ip=" + ipAddress;
                System.Net.WebClient wc = new System.Net.WebClient();

                string str = wc.DownloadString(url);
                return ParseHtmlResult(str);
            }
            catch (Exception ex) { return "Could not find location by IP stuff from external source. Error: " + ex.Message; }
        }

        private static string ParseHtmlResult(string result)
        {
            string temp = result.Substring(result.IndexOf("<table cellspacing=\"0\" width=\"650\">"));
            temp = temp.Substring(0, temp.IndexOf("</table>") + 8);
            return temp;
        }

        private void BuildVisitorFields()
        {
            var data = Common.DBModel();

            db.visitor vis = data.visitors.First(x => x.ip_address == _ipAddress);
            vis.details_html = GetLocationFromIPLocationTools(_ipAddress);

            // TODO:
            // here is where we'll parse out the fields later

            data.SubmitChanges();
            
        }

        internal static void ExecuteEmailSendAsync(string ip)
        {
            Meta m = new Meta(ip);
            System.Threading.Thread t = new System.Threading.Thread(m.ExecuteEmailSend);
            t.Start();
        }

        internal static void ExecuteEmailSendAsync(string ip, string emailTo)
        {
            Meta m = new Meta(ip, emailTo);
            System.Threading.Thread t = new System.Threading.Thread(m.ExecuteEmailSend);
            t.Start(); 
        }

        internal static void ExecuteEmailSendAsync(string ip, string emailTo, bool sendAddlInfo)
        {
            Meta m = new Meta(ip, emailTo, sendAddlInfo);
            System.Threading.Thread t = new System.Threading.Thread(m.ExecuteEmailSend);
            t.Start();
        }

        internal static void AddNewVisitorAsync(string ip)
        {
            AddNewVisitor(ip);

            Meta m = new Meta(ip);
            System.Threading.Thread t = new System.Threading.Thread(m.BuildVisitorFields);
            t.Start();
        }

        private static void AddNewVisitor(string ip)
        {
            if (!NewVisitor(ip))
                return;

            var data = Common.DBModel();

            db.visitor visitor = new db.visitor();
            visitor.ip_address = ip;
            // this takes a long time to do syncronously, so we'll add the record then update async.
            //visitor.details_html = GetLocationFromIPLocationTools(_ipAddress);
            visitor.first_request_dt = DateTime.Now;

            data.visitors.InsertOnSubmit(visitor);
            data.SubmitChanges();
        }

        internal static void LogRequest(string ip, string url, string controlDesc)
        {
            if (Common.DBModel().visitors.Count(x => x.ip_address == ip) > 0)
            {
                var data = Common.DBModel();

                db.visitorRequestLog log = new db.visitorRequestLog();
                log.ip_address = ip;
                log.request_datetime = DateTime.Now;
                log.request_url = url;
                log.request_page = GetPageFromUrl(url);
                log.username = Authentication.GetAuthenticatedUserName();
                log.control_desc = controlDesc;

                data.visitorRequestLogs.InsertOnSubmit(log);
                data.SubmitChanges();
            }
        }

        private static string GetPageFromUrl(string url)
        {
            string lastBit = url.Split('/')[url.Split('/').Length - 1];
            if (lastBit.Contains("?")) lastBit = lastBit.Substring(0, lastBit.IndexOf("?"));
            return lastBit;
        }

        internal static bool NewVisitor(string ip)
        {
            return Common.DBModel().visitors.Count(x => x.ip_address == ip) == 0;
        }

        

        //internal static void LogError(string errorMessage)
        //{
        //    string filePath = @"C:\FTP\Eric\SiteErrors\" + DateTime.Now.ToFileTime() + ".txt";
        //    System.IO.File.WriteAllText(filePath, errorMessage);
        //}

        string URL { get; set; }
        internal static void CallCacheUrlAsync()
        {
            string querystring = HttpContext.Current.Session["autoweek"] != null ?
                "?week=" + HttpContext.Current.Session["autoweek"].ToString() :
                "";
            string url = LibCommon.WebsiteUrlRoot() + "LoadCache.aspx" + querystring;

            Meta m = new Meta();
            m.URL = url;
            System.Threading.Thread t = new System.Threading.Thread(m.DoCallURLWork);
            t.Start();
        }
        private void DoCallURLWork()
        {
            if (!string.IsNullOrEmpty(this.URL))
            {
                System.Net.WebClient client = new System.Net.WebClient();
                client.DownloadString(this.URL);
            }
        }

        internal static void LoadCache(int? week = null)
        {
            int sportId = Common.GetCurrentSportId();
            int defaultWeek = Common.GetDefaultWeek(sportId: sportId);
            DoLoadCacheWorkForWeek(defaultWeek);

            if (week.HasValue)
                DoLoadCacheWorkForWeek(week.Value);
        }
        private static void DoLoadCacheWorkForWeek(int week)        
        {
            CacheHelper.GetCachedCummmulativeGridViewStatsDataSource(week, null, new int[] { (int)WeekTypes.RegularSeason, (int)WeekTypes.PostSeason }, clearCache: true);
            CacheHelper.GetCachedGridViewMatchupsDataSource(week, clearCache: true);
            CacheHelper.GetCachedGridViewWeeklyStatsDataSource(week, clearCache: true);
            try
            {
                int yearId = Common.DBModel().NFL_weeks.First(x => x.week == week).year_id.Value;
                CacheHelper.GetCachedGridViewTheBetDataSource(yearId, clearCache: true);
            }
            catch { }
        }

        internal static void LoadCacheKeys()
        {
            var enumerator = HttpContext.Current.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                switch (enumerator.Key.ToString().Split('-')[0])
                {
                    case CacheHelper.GridViewWeeklyStatsDataSourceKey:
                        CacheHelper.GetCachedGridViewWeeklyStatsDataSource(0, clearCache: true, overrideKey: enumerator.Key.ToString());
                        break;
                    case CacheHelper.GridViewCummmulativeStatsDataSourceKey:
                        CacheHelper.GetCachedCummmulativeGridViewStatsDataSource(0, null, null, clearCache: true, overrideKey: enumerator.Key.ToString());
                        break;
                    case CacheHelper.GridViewTheBetDataSourceKey:
                        CacheHelper.GetCachedGridViewTheBetDataSource(0, clearCache: true, overrideKey: enumerator.Key.ToString());
                        break;
                    case CacheHelper.GridViewMatchupsDataSourceKey:
                        CacheHelper.GetCachedGridViewMatchupsDataSource(0, clearCache: true, overrideKey: enumerator.Key.ToString());
                        break;
                }
            }
        }
    }
}