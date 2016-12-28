using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;

namespace HomeWebApp
{
    /// <summary>
    /// Summary description for Batch
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Batch : System.Web.Services.WebService
    {

        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public string Test(string parameter)        
        {
            return "Your parameter was " + parameter + "!";
        }


        [WebMethod]
        public string SendEmail(string to, string subject, string body, string displayName)
        {
            return Common.SendEmail(to, subject, body, displayName);
        }

        [WebMethod]
        [System.Web.Script.Services.ScriptMethod(UseHttpGet = true)]
        public void AddExpandedTopicToUserSetting(string id, string username)
        {
            var data = Common.DBModel();

            db.userSetting newSetting = new db.userSetting();
            newSetting.username = username;
            newSetting.settingKey = "expandedTopic";
            newSetting.settingValue = id;
            newSetting.insert_dt = DateTime.Now;
            newSetting.update_dt = DateTime.Now;
            newSetting.active_flag = true;

            data.userSettings.InsertOnSubmit(newSetting);
            data.SubmitChanges(); 
        }

        [WebMethod]
        [System.Web.Script.Services.ScriptMethod(UseHttpGet = true)]
        public void RemoveExpandedTopicFromUserSetting(string id, string username)
        {
            var data = Common.DBModel();
            var userSettings = data.userSettings.Where(x => x.id == Convert.ToInt32(id) && x.username == username && x.active_flag == true);
            bool changeMade = false;

            foreach (var setting in userSettings)
            {
                setting.active_flag = false;
                changeMade = true;
            }
            
            if (changeMade)
                data.SubmitChanges();
        }

    

    }
}
