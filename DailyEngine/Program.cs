using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HomeAppsLib;
using HomeAppsLib.TypeExtensions;

namespace DailyEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                LibCommon.UpdateNFLMatchups(false);
                return;

                DateTime start = DateTime.Now; // start timestamp

                // run program                
                int picksRemindersCount = PicksReminders.SendPicksReminders();
                int formRecapCount = ForumRecaps.SendDailyForumRecaps();

                DateTime finish = DateTime.Now; // finish timestamp



                // report results

                string type = "Daily Engine Version " + GetVersion();
                string desc = "Started {" + start + "}, Completed {" + finish + "}, Pick Reminders {" + picksRemindersCount + " sent}, Discussion Recaps {" + formRecapCount + " sent}.";

                LogRun(type, desc);
                if (DateTime.Now  < Convert.ToDateTime("2/8/14"))
                    LibCommon.SendEmail("eric@hackerdevs.com", type, "Should be last email for a while!<hr>" + desc, "Daily Engine");
            }
            catch (Exception ex)
            {
                LibCommon.SendEmail("eric@hackerdevs.com", "ERROR in Daily Engine", ex.ToString(), "Daily Engine");
            }
        }
        private static string GetVersion()
        {
            return "14.2.6";
        }
        private static void LogRun(string type, string desc)
        {
            var data = LibCommon.DBModel();
            HomeAppsLib.db.log log = new HomeAppsLib.db.log();
            log.type = type;
            log.description = desc;
            log.LogDate = DateTime.Now;
            data.logs.InsertOnSubmit(log);
            data.SubmitChanges();
        }

        
    }
}