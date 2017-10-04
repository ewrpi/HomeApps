using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Meta.LogError("Testing error message " + DateTime.Now);
            try
            {                
                string ip = Request.ServerVariables["remote_addr"];
                Common.CheckForUserToVisitorInsert(Authentication.GetAuthenticatedUserName(), ip);

                if (!Page.IsPostBack)
                {
                    Meta.LogRequest(ip, Request.RawUrl, "Not Postback");

                    if (Meta.NewVisitor(ip))
                    {
                        //Meta.ExecuteEmailSendAsync(ip);
                        Meta.AddNewVisitorAsync(ip);
                    }                    
                }

                else
                {
                    Control control = FindWhatControlDidPostBack();
                    Meta.LogRequest(ip, Request.RawUrl, control.GetType() + " " + control.ID);
                }

                //Meta.UpdateNFLMatchups(true);
                //Meta.CallCacheUrlAsync();
            }

            catch (Exception ex) 
            {
                string emailStatus = Common.SendEmail("eric@hackerdevs.com", "exception thrown in site.Master form load", ex.ToString(), "HomeWebAppException");

                //if (emailStatus != "Success")
                    //Meta.LogError("Error sending email {" + emailStatus + "}. \n\n\nThe email was reporting exception in site.Master form load {" + ex.ToString() + "}.");
            }
                
        }

        private Control FindWhatControlDidPostBack()
        {
            return Common.FindWhatControlDidPostBack(this.Page);            
        }        

        
    }
}