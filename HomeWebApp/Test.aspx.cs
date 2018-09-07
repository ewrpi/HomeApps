using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DateTime delay = DateTime.Now.AddSeconds(15);
            while(DateTime.Now < delay)
            {
                System.Threading.Thread.Sleep(1);
            }
            HomeAppsLib.LibCommon.SendCoryText(172);
        }
    }
}