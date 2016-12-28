using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class LoadCache : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["key"] != null)
            {
                Meta.LoadCacheKeys();
            }
            else
            {
                int? week = null;
                int temp;
                if (Request.QueryString["week"] != null && int.TryParse(Request.QueryString["week"], out temp))
                    week = temp;

                Meta.LoadCache(week);
            }
        }
    }
}