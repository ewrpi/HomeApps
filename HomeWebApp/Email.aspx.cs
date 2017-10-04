using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class Email : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session["m"] != null)
                lblMessageFromQueryString.Text = HttpContext.Current.Session["m"].ToString();
            else if (Request.QueryString["m"] != null)
            {
                HttpContext.Current.Session["m"] = Request.QueryString["m"].ToString();
                lblMessageFromQueryString.Text = HttpContext.Current.Session["m"].ToString();
            }
        }

        protected void btnSendEmail_Click(object sender, EventArgs e)
        {
            string user = Authentication.GetAuthenticatedUserName() ?? "AnonymousUser";
            string result = Common.SendEmail("eric@hackerdevs.com", lblMessageFromQueryString.Text, txtEmailBody.Text, user);

            if (result.ToLower() != "success")
            {
                lblSendStatus.Text = result;// +" - your request has been logged on the server.";
                //AppendTextToLogFile(txtEmailBody.Text, user);
            }
            else
                lblSendStatus.Text = "An email has been send to the system administrator on your behalf.";
        }

        private void AppendTextToLogFile(string text, string user)
        {
            string dirName = "nflpickExpirationRequests";

            if (!System.IO.Directory.Exists(@"C:\FTP\Eric\" + dirName)) System.IO.Directory.CreateDirectory(@"C:\FTP\Eric\" + dirName);

            System.IO.StreamWriter sw = new System.IO.StreamWriter(@"C:\FTP\Eric\" + dirName + "\\" + user + ".txt", true);
            sw.WriteLine("**************" + DateTime.Now + "**************");
            sw.WriteLine(text);
            sw.Close();
        }
    }
}