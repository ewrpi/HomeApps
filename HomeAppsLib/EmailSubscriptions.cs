using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib
{
    public class EmailSubscriptions
    {
        public const string EMAIL_SUB_SETTING_KEY_PREFIX = "SubscribedToEmail-";

        public static db.user[] GetSubscribedUsers(EmailSubscriptionType subType)
        {
            var names = LibCommon.DBModel().userSettings
                .Where(x => x.settingKey == EMAIL_SUB_SETTING_KEY_PREFIX + (int)subType && x.active_flag)
                .Select(x => x.username)
                .ToArray();

            return LibCommon.DBModel().users.Where(x => names.Contains(x.name) && x.email != null && x.IsActive).OrderBy(u => u.registration_dt).ToArray();
        }

        public static void AddSubscription(int subTypeId, string username)
        {
            AddRemoveSubscription(subTypeId, username, true);
        }
        public static void RemoveSubscription(int subTypeId, string username)
        {
            AddRemoveSubscription(subTypeId, username, false);
        }

        private static void AddRemoveSubscription(int subTypeId, string username, bool add)
        {
            string settingKey = EMAIL_SUB_SETTING_KEY_PREFIX + subTypeId;
            var data = LibCommon.DBModel();

            // first remove any existing
            var existingSubs = data.userSettings
                .Where(x => x.active_flag && x.settingKey == settingKey && x.username == username);
            foreach (var sub in existingSubs)
            {
                sub.active_flag = false;
                sub.update_dt = DateTime.Now;
            }

            // if add, then add it
            if (add)
            {
                db.userSetting sub = new db.userSetting();
                sub.settingKey = settingKey;
                sub.settingValue = string.Empty;
                sub.username = username;
                sub.insert_dt = DateTime.Now;
                sub.update_dt = DateTime.Now;
                sub.active_flag = true;
                data.userSettings.InsertOnSubmit(sub);
            }

            data.SubmitChanges();
        }

        public static bool UserHasSubscription(int subTypeId, string username)
        {
            return LibCommon.DBModel().userSettings.Count(x => x.settingKey == EMAIL_SUB_SETTING_KEY_PREFIX + subTypeId && x.username == username && x.active_flag) > 0;
        }

        public static string GetEmailSubDescription(int subTypeId)
        {
            var sub = LibCommon.DBModel().emailSubscriptionTypes.FirstOrDefault(x => x.Id == subTypeId);
            if (sub != null)
                return sub.Name;
            else return null;
        }

        public static string LinkToUnsubscripe(db.user user, int subTypeId)
        {
            return LinkToUnsubscripe(user.encPW, subTypeId);
        }
        public static string LinkToUnsubscripe(string userKey, int subTypeId)
        {
            string result = LibCommon.WebsiteUrlRoot() + "UpdateFromLink.aspx?";

            result += "commandType=1";
            result += "&emailType=" + subTypeId;
            result += "&userKey=" + userKey;

            return result;
 
        }

        public static int[] AllSubscriptionTypeIds()
        {
            return LibCommon.DBModel().emailSubscriptionTypes.Select(x => x.Id).ToArray();
        }

        public static string GenerateDiscPostEmailBodyText(db.NFL_forum post)
        {
            string result = string.Empty;
            string diff = post.ref_id.HasValue ? " replied on " : " started the ";

            result += "<ul>";
            result += "<li><b>" + post.username + diff + "topic \"" + post.TopicDescription + "\" at " + post.insert_dt.Value.ToShortTimeString() + " (" + post.NFL_week.text + ").</b></li>";
            result += "<ul><li>" + post.DisplayComment + "</li></ul>";
            result += "</ul>";

            return result;
        }
    }    
}
