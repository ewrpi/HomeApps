using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HomeAppsLib;
using HomeAppsLib.TypeExtensions;

namespace DailyEngine
{
    class ForumRecaps
    {
        public static int SendDailyForumRecaps()
        {
            int emailCount = 0;
            bool postsMade;

            string emailBody = GetEmailBody(out postsMade);

            if (postsMade)
            {
                foreach (HomeAppsLib.db.user user in EmailSubscriptions.GetSubscribedUsers(EmailSubscriptionType.DailyForumRecap))
                {
                    LibCommon.SendEmail(user.email, "Daily Disussion Recap", emailBody.Replace("XXXXXXXXXX", user.encPW), "The Wright Picks");
                    emailCount++;
                }
            }

            return emailCount;
        }

        private static string GetEmailBody(out bool postsMade)
        {
            DateTime start = DateTime.Now.AddDays(-1).Date;
            DateTime end = start.AddDays(1);

            string result = "<span style='font-size:x-large;'>Discussion Recap For " + start.ToLongDateDisplay() + ".</span><br><br>";
            int numberOfPosts = 0;
            foreach (HomeAppsLib.db.NFL_forum post in LibCommon.DBModel().NFL_forums.Where(x => x.insert_dt >= start && x.insert_dt < end).OrderBy(x => x.insert_dt))
            {
                numberOfPosts++;

                result += HomeAppsLib.EmailSubscriptions.GenerateDiscPostEmailBodyText(post);                

                result += "<br><br>";
            }
            if (numberOfPosts == 0)
                result += "There are no discussions to report.<br><br>";

            postsMade = numberOfPosts > 0;

            result += "<a href='" + LibCommon.WebsiteUrlRoot() +"'>Click here to log in and view all discussions.</a><br><br>";
            result += "Thanks,<br>The Wright Picks";

            result += "<br><br><br><span style='font-size:small;'>P.S. If you'd like to unsubscribe from this email, click <a href='" + EmailSubscriptions.LinkToUnsubscripe("XXXXXXXXXX", (int)EmailSubscriptionType.DailyForumRecap) + "'>here.</a></span>";

            return result;
        }

    }
}
