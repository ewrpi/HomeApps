using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HomeAppsLib;
using HomeAppsLib.TypeExtensions;

namespace DailyEngine
{
    class PicksReminders
    {
        public static int SendPicksReminders()
        {
            DateTime aFewDaysOut = DateTime.Now.AddDays(1).ToEndOfDay();
            var weeksAboutToExpire = LibCommon.DBModel().NFL_weeks.Where(x => x.exp_dt > DateTime.Now && x.exp_dt < aFewDaysOut && x.week > 0);

            int count = 0;

            foreach (var weekAboutToExpire in weeksAboutToExpire)
                count += SendRemindersForWeek(weekAboutToExpire);

            return count;
        }

        private static int SendRemindersForWeek(HomeAppsLib.db.NFL_week weekAboutToExpire)
        {
            int count = 0;
            
            if (weekAboutToExpire != null && weekAboutToExpire.NFL_year.NFL_sport.id == (int)Sports.NFL)
            {
                count += SendReminderEmails(weekAboutToExpire, EmailSubscriptionType.NFLPicksReminderEmail);
            }
            else if (weekAboutToExpire != null && weekAboutToExpire.NFL_year.NFL_sport.id == (int)Sports.MLB)
            {
                count += SendReminderEmails(weekAboutToExpire, EmailSubscriptionType.MLBPicksReminderEmail);
            }

            return count;
        }

        private static int SendReminderEmails(HomeAppsLib.db.NFL_week weekAboutToExpire, EmailSubscriptionType emailSubscriptionType)
        {
            int count = 0;

            foreach (var user in EmailSubscriptions.GetSubscribedUsers(emailSubscriptionType))
            {
                if (LibCommon.DBModel().NFL_userPicks.Count(x => x.week == weekAboutToExpire.week && x.username == user.name) == 0)
                {
                    string to = LibCommon.IsDevelopmentEnvironment() ? "eric@hackerdevs.com" : user.email;

                    int daysLeft = (weekAboutToExpire.exp_dt.Date - DateTime.Now.Date).Days;

                    string subject = "Only " + daysLeft + " days left to make your picks!";
                    if (daysLeft == 1)
                        subject = subject.Replace("days", "day");
                    if (daysLeft == 0)
                        subject = "Picks expire today!";

                    //if (DateTime.Now < new DateTime(2016, 9, 10))
                    //    subject = "FEATURING CORY QUICK PICKS!! " + subject;

                    string body = "Hi, " + user.name + "!<br><br>";
                    body += "Remember, picks for <b>" + weekAboutToExpire.text + "</b> will expire on " + weekAboutToExpire.exp_dt.ToLongDateTimeDisplay() + "!<br><br>";
                    body += "<span style='font-size:x-large;'>Be sure to make your picks before then!</span><br>";
                    body += "<br><a href='" + LibCommon.WebsiteUrlRoot() + "nflpicks.aspx?quickpicks=true&autoweek=" + weekAboutToExpire.week + "&sso=" + LibCommon.SSOUserKey(user) + "' style='font-size:xx-large;'>CLICK HERE FOR NEW CORY QUICK PICKS!</span><br><br>";
                    body += "<span style='font-size:large;'><a href='" + LibCommon.WebsiteUrlRoot() + "'>Click here to log in and make your picks now</a></span><br><br>";
                    body += "Thanks,<br>The Wright Picks";
                    body += "<br><br><br><span style='font-size:small;'>P.S. If you'd like to unsubscribe from this email, click <a href='" + EmailSubscriptions.LinkToUnsubscripe(user, (int)emailSubscriptionType) + "'>here.</a></span>";

                    LibCommon.SendEmail(to, subject, body, "The Wright Picks");

                    if (user.name.ToLower() == "cory")
                    {
                        HomeAppsLib.LibCommon.SendCoryText(weekAboutToExpire.week);
                    }

                    count++;
                }
            }

            return count;
        }        
    }
}
