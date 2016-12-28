using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class UpdateFromLink : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int? commandType = (int?)FromQueryString("commandType");
            lblResult.Text = string.Empty;

            if (commandType.HasValue)
            {
                switch (commandType.Value)
                {
                    case 1: // unsubscribe from email
                        int? emailType = (int?)FromQueryString("emailType");
                        string userKey = FromQueryString("userKey").ToString();
                        string username = HomeAppsLib.LibCommon.GetUserNameFromEncryptedPassword(userKey);
                        if (!string.IsNullOrEmpty(username) && emailType.HasValue && HomeAppsLib.EmailSubscriptions.AllSubscriptionTypeIds().Contains(emailType.Value))
                        {
                            HomeAppsLib.EmailSubscriptions.RemoveSubscription(emailType.Value, username);
                            lblResult.Text = "You have successfully been removed from the \"" + HomeAppsLib.EmailSubscriptions.GetEmailSubDescription(emailType.Value) + "\" email subscription.";
                            AddHowCouldYouDoThatToMeImage();
                        }
                        break;
                }
            }
            else
                lblResult.Text = "Invalid command type";

            if (lblResult.Text.Length == 0)
                lblResult.Text = "No transaction was performed.";
        }

        private void AddHowCouldYouDoThatToMeImage()
        {
            System.Web.UI.WebControls.Image img = new Image();
            img.ImageUrl = "/images/HowCouldYouDoThatToMe.PNG";
            img.Width = new Unit(350);
            img.BorderColor = System.Drawing.Color.Black;
            img.BorderWidth = new Unit(5);
            img.BorderStyle = BorderStyle.Solid;

            pnl.Controls.Add(img);

        }

        private object FromQueryString(string key)
        {
            string value = Request.QueryString[key];
            if (string.IsNullOrEmpty(value))
                return null;

            try
            {
                switch (key)
                {
                    case "commandType":
                    case "emailType":
                        return Convert.ToInt32(value);
                    default:
                        return value;

                }
            }
            catch { return null; }
        }
    }
}