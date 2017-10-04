using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using HomeAppsLib;

namespace HomeWebApp
{
    public partial class Eric : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.MaintainScrollPositionOnPostBack = true;
            lblMachineName.Text = Environment.MachineName;

            if (!Page.IsPostBack)
            {
                cbTestMode.Checked = Common.IsAppInTestMode();
                cbRapidFire.Checked = Common.IsAppInMode("NFL_PICKS_MOBILE_RAPID_FIRE");

                var data = Common.DBModel();
                gvVisitors.DataSource = (from v in data.visitors                                         
                                        select new 
                                        { 
                                            IPAddress = v.ip_address, 
                                            FirstRequest = v.first_request_dt, 
                                            LastRequest = (data.visitorRequestLogs.Where(x => x.ip_address == v.ip_address).Max(x => x.request_datetime)),
                                            Message = v.message,
                                            TotalRequests = (data.visitorRequestLogs.Count(x => x.ip_address == v.ip_address)) 
                                        }).OrderByDescending(x => x.LastRequest);
                gvVisitors.DataBind();

                lblTotalVisitors.Text = "We have had " + GetNumberTotalVisitors() + " visitors total!";

                lblSession.Text = GenerateSessionObjectsLabel();
                lblAllUsers.Text = string.Join("<br>", Common.GetAllLoggedInUsers().ToArray());
                lblHeaders.Text = GetHeadersText();

                if (rodneyOneClick())
                {
                    txtPW.Text = "threwwu7";
                    btnDisplay_Click(this, null);
                }
                else
                {
                    txtPW.Text = string.Empty;
                    lblError.Text = "test";
                }                
            }

            foreach (string filename in System.IO.Directory.GetFiles(Server.MapPath("~/queries")))
            {
                Button querybutton = new Button();
                querybutton.Text = System.IO.Path.GetFileNameWithoutExtension(filename);
                querybutton.CommandArgument = System.IO.Path.GetFileName(filename);
                querybutton.Command += new CommandEventHandler(querybutton_Command);

                pnlQuerys.Controls.Add(querybutton);
                pnlQuerys.Controls.Add(new System.Web.UI.HtmlControls.HtmlGenericControl("br"));
            }
        }

        private string GetHeadersText()
        {
            string result = string.Empty;

            foreach (string key in Request.Headers.AllKeys)
            {
                result += "Key: " + key + ", Value: " + Request.Headers[key] + "<br>";
            }
            return result;
        }

        private string GenerateSessionObjectsLabel()
        {
            string text = "<br/>";

            foreach (string key in Session.Keys)
                text += key + ": " + Session[key] + "<br/>";

            return text;

        }

        void querybutton_Command(object sender, CommandEventArgs e)
        {
            txtQuery.Text = System.IO.File.ReadAllText(Server.MapPath("~/queries") + @"\" + e.CommandArgument);
        }
        
        private int GetNumberTotalVisitors()
        {
            return Common.DBModel().visitors.Count();
        }

        private bool rodneyOneClick()
        {
            if (Request.QueryString["oneclick"] == null) return false;            

            Encryption enc = new Encryption();
            try { return enc.Decrypt(Request.QueryString["oneclick"].ToString()) == "rodneyOneClick!"; }
            catch (Exception ex) { lblError.Text = ex.Message; return false; }
        }

        protected void btnDisplay_Click(object sender, EventArgs e)
        {
            if (txtPW.Text == "threwwu7")
            {
                pnl_main.Visible = true;
                txtPW.Text = string.Empty;
            }
        }

        protected void btnSendIPNotificationEmail_Click(object sender, EventArgs e)
        {
            Meta.ExecuteEmailSendAsync(txtIPAddress.Text, "eric@hackerdevs.com", cb_sendAddlInfo.Checked);
            txtIPAddress.Text = string.Empty;
            cb_sendAddlInfo.Checked = false;
        }

        protected void gvVisitors_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "select")
            {
                string ip = gvVisitors.Rows[Convert.ToInt32(e.CommandArgument)].Cells[2].Text;

                gvRequestLog.DataSource = Common.DBModel().visitorRequestLogs.Where(x => x.ip_address == ip).OrderByDescending(x => x.request_datetime);
                gvRequestLog.DataBind();

                gvComments.DataSource = Common.DBModel().visitorCustomMessageHistories.Where(x => x.ip_address == ip).OrderByDescending(x => x.update_dt);
                gvComments.DataBind();

                HttpContext.Current.Session["ipdata"] = FormatIPStuff(Common.DBModel().visitors.First(x => x.ip_address == ip).details_html);
            }

            if (e.CommandName == "retry")
            {
                string ip = gvVisitors.Rows[Convert.ToInt32(e.CommandArgument)].Cells[2].Text;

                string html = Meta.GetLocationFromIPLocationTools(ip);

                var data = Common.DBModel();
                var vis = data.visitors.First(x => x.ip_address == ip);
                vis.details_html = html;
                data.SubmitChanges();

                HttpContext.Current.Session["ipdata"] = html;
            }
        }

        private string FormatIPStuff(string html)
        {
            while (html.Contains("<tr>"))
            {
                int index = html.IndexOf("<tr>");
                string before = html.Substring(0, index);

                string on = html.Substring(index);
                on = on.Substring(0, on.IndexOf("</tr>") + 5);

                string after = html.Substring(before.Length + on.Length);

                html = before + after;
            }

            return html;
        }

        protected void gvComments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "revert")
            {
                int id = Convert.ToInt32(gvComments.Rows[Convert.ToInt32(e.CommandArgument)].Cells[1].Text);
                var commentItem = Common.DBModel().visitorCustomMessageHistories.First(x => x.id == id);

                Common.UpdateUserCustomMessage(commentItem.ip_address, commentItem.message);
                
            }
        }

        protected void btnRunQuery_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection(Common.DBConnectionString());
            cn.Open();
            System.Data.SqlClient.SqlCommand cm = new System.Data.SqlClient.SqlCommand(txtQuery.Text, cn);
            cm.ExecuteNonQuery();
            cn.Close();
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlDataAdapter sda = new System.Data.SqlClient.SqlDataAdapter(txtQuery.Text, Common.DBConnectionString());
            System.Data.DataTable dt = new System.Data.DataTable();
            sda.Fill(dt);
            
            gvQueryResults.DataSource = dt;
            gvQueryResults.DataBind();

            for (int i=1; i<gvQueryResults.Rows.Count; i++)
            {
                for (int j=0; j<gvQueryResults.Columns.Count; j++)
                {
                    if (IsHtmlField(gvQueryResults.Rows[0].Cells[j].Text))
                        gvQueryResults.Rows[i].Cells[j].Text = System.Web.HttpUtility.HtmlDecode(gvQueryResults.Rows[i].Cells[j].Text);
                }
            }

        }

        private bool IsHtmlField(string field)
        {
            switch (field) 
            {
                case "EmailBody":
                    return true;
                default:
                    return false;
            }
        }

        protected void btnSaveQuery_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(Server.MapPath("~/queries") + @"\" + txtFileName.Text + ".sql", txtQuery.Text);
        }

        protected void btnThrowException_Click(object sender, EventArgs e)
        {
            throw new Exception("You clicked the button to throw an exception");
        }

        protected void cbTestMode_CheckedChanged(object sender, EventArgs e)
        {
            Common.ChangeAppMode(cbTestMode.Checked);
        }

        protected void cbRapidFire_CheckedChanged(object sender, EventArgs e)
        {
            Common.ChangeAppMode(cbRapidFire.Checked, "NFL_PICKS_MOBILE_RAPID_FIRE");
        }

        protected void btnAutoEnrollAllSubs_Click(object sender, EventArgs e)
        {
            foreach (HomeAppsLib.db.user user in HomeAppsLib.LibCommon.DBModel().users.Where(x => x.email != null && x.IsActive))
            {
                foreach (HomeAppsLib.db.emailSubscriptionType sub in HomeAppsLib.LibCommon.DBModel().emailSubscriptionTypes.Where(x => x.autoEnroll))
                {
                    HomeAppsLib.EmailSubscriptions.AddSubscription(sub.Id, user.name);
                }
            }
        }        
    }
}