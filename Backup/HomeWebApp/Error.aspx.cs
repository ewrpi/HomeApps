using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string error = Request.QueryString["m"] != null ? Request.QueryString["m"].ToString() : "Error message was not sent.";
            lblError.Text = "An error occurred in your last request: " + error;
        }
    }
}