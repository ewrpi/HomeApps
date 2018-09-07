using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string ip = Request.ServerVariables["remote_addr"];
            lbl_IPAddress.Text = ip;
            lbl_remoteHost.Text = Request.ServerVariables["REMOTE_HOST"];

            //HomeAppsLib.LibCommon.SendEmail("eric@hackerdevs.com", "testing zoho email", "did you get it?", "The Wright Picks");

            //if (!Page.IsPostBack)
            //{
            //    if (Common.UserHasCustomMessage(ip))
            //    {
            //        pnlHasMessage.Visible = true;
            //        pnlNoMessage.Visible = false;
            //        txtExistingMessage.Text = Common.GetUserCustomMessage(ip);
            //    }

            //    else
            //    {
            //        pnlHasMessage.Visible = false;
            //        pnlNoMessage.Visible = true;
            //    }
            //}
        }        

        protected void btn_send_Click(object sender, EventArgs e)
        {
           // Common.SendEmail(txt_to.Text, txt_subject.Text, txt_body.Text, txt_displayname.Text);
        }

        protected void btnSubmitMessage_Click(object sender, EventArgs e)
        {
            Common.AddVisitorCustomMessage(Request.ServerVariables["remote_addr"].ToString(), txtMessage.Text);
            Response.Redirect(Request.RawUrl);
        }

        protected void btnModifyMessage_Click(object sender, EventArgs e)
        {
            switch (btnModifyMessage.Text.ToLower())
            {
                case "change":
                    txtExistingMessage.Enabled = true;
                    btnModifyMessage.Text = "Submit";
                    break;
                case "submit":
                    Common.UpdateUserCustomMessage(Request.ServerVariables["remote_addr"].ToString(), txtExistingMessage.Text);
                    Response.Redirect(Request.RawUrl);
                    break;
            }
        }
        
    }
}