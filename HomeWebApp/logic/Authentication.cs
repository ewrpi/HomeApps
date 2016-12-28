using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using HomeAppsLib;

namespace HomeWebApp
{
    public class Authentication
    {
        //public static string AUTHENTICATED_USER_STRING = "authenticateduser";
        public static string AUTHENTICATED_USER_STRING = "D11B8CDC-803E-4777-8DF8-A99E192708B4";
        public static string AUTHENTICATED_USER_ENC_PASSWORD_STRING = "9C0AE5CB-1B1C-4567-842B-25D11BEFB65C";

        internal static void RegisterNewUser(string username, string password, string ip, string emailAddress, string birthday)
        {
            Encryption enc = new Encryption();

            var data = HomeAppsLib.LibCommon.DBModel();
            HomeAppsLib.db.user newUser = new HomeAppsLib.db.user();

            newUser.name = username[0].ToString().ToUpper() + username.Substring(1);
            newUser.encPW = enc.Encrypt(password);
            newUser.ip_address = ip;
            newUser.registration_dt = DateTime.Now;
            newUser.IsActive = true;
            if (birthday.Trim().Length > 0)
                newUser.birthday = Convert.ToDateTime(birthday);
            
            if (!string.IsNullOrEmpty(emailAddress)) newUser.email = emailAddress;

            data.users.InsertOnSubmit(newUser);
            data.SubmitChanges();

            SetAuthenticatedUser(newUser.name);
        }

        public static void SetAuthenticatedUser(string username)
        {
            if (Common.DBModel().users.Count(x => x.name == username) > 0)
            {
                HttpContext.Current.Session[AUTHENTICATED_USER_STRING] = username;
            }
            else LogUserOut();
        }

        public static string GetAuthenticatedUserName()
        {
            //if (Environment.MachineName.ToLower() == "sf-11789")
            //    return "Eric";
            
            var authUser = HttpContext.Current.Session[AUTHENTICATED_USER_STRING];            

            if (authUser != null)
            {
                return HttpContext.Current.Session[AUTHENTICATED_USER_STRING].ToString();
            }
            else
                return null;
        }

        internal static void LogUserOut()
        {
            while (HttpContext.Current.Session.Keys.Count > 0)
                HttpContext.Current.Session.Remove(HttpContext.Current.Session.Keys[0]);
        }

        internal static string[] CurrentUsersList(bool toLower)
        {
            List<string> users = new List<string>();

            foreach (var user in Common.DBModel().users)
            {
                if (toLower) users.Add(user.name.ToLower());
                else users.Add(user.name);
            }

            return users.ToArray();
        }

        internal static bool ValidateUserSSO(string ssoKey)
        {
            try
            {
                Encryption enc = new Encryption();
                string pair = enc.Decrypt(ssoKey);
                string[] parts = pair.Split('|');
                if (parts.Length != 2)
                    return false;
                else
                    return ValidateUser(parts[0], parts[1], true);
            }
            catch { return false; }
        }

        internal static bool ValidateUser(string username, string password, bool encrypted)
        {
            var users = Common.DBModel().users;

            if (users.Count(x => x.name == username) > 0)
            {
                Encryption enc = new Encryption();

                var user = users.FirstOrDefault(x => x.name == username && x.IsActive);

                bool result = false;
                if (user != null)
                {
                    if (!encrypted) result = enc.Encrypt(password) == user.encPW;
                    else result = password == user.encPW;

                    if (result) SetAuthenticatedUser(user.name);
                }

                return result;
            }

            return false;
        }

        internal static bool IsUserAuthenticated()
        {
            return GetAuthenticatedUserName() != null;
        }

        public static List<string> AdminUsers()
        {
            //return new string[] { System.Guid.NewGuid().ToString() }.ToList();
            return new string[] { "Eric","Blaine"}.ToList();
        }
        public static bool IsAdminUser()
        {
            return AdminUsers().Contains(HomeWebApp.Authentication.GetAuthenticatedUserName());
        }
        public static List<string> BetUsers(int yearId)
        {
            //return new string[] { System.Guid.NewGuid().ToString() }.ToList();

            var year = Common.DBModel().NFL_years.FirstOrDefault(y => y.id == yearId);
            if (year != null && !string.IsNullOrEmpty(year.betUsers))
            {
                return year.betUsers.Split(',').ToList();
            }

            return new List<string>();

            //switch (yearId)
            //{
            //    case 2: // 2013-14
            //        return new string[] { "Eric", "Blaine", "Hankster", "Jason Wright" }.ToList();
            //    case 4: // 2014-15
            //        return new string[] { "Eric", "Blaine", "Hankster", "Jason Wright", "LeRoy", "Herfer", "Janet" }.ToList();
            //    case 5: // 2015-16
            //        return new string[] { "Eric", "Blaine", "Hankster", "Jason Wright", "LeRoy", "Herfer", "Janet", "Todd Johnston" }.ToList();
            //    default:
            //        return new List<string>();
            //}
            
        }
        public static bool IsABetUser(int yearId)
        {
            return BetUsers(yearId).Contains(HomeWebApp.Authentication.GetAuthenticatedUserName());
        }
    }
}