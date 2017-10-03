using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class Apps : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpContext.Current.Session["apps"] = GetAppsHtml();
        }

        private string GetAppsHtml()
        {
            string htmlString = "<div align='center'>";
            htmlString += "<table bgcolor='#F7F7DE' border='1px' cellpadding='5' cellspacing='0' style='border: solid 1px Silver; font-size: medium;'>";

            htmlString += "<tr><td>App Name</td><td>Description</td><td></td></tr>";

            foreach (string appDir in System.IO.Directory.GetDirectories(physicalAppsRoot()))
            {
                string dirDesc = GetDirNameShort(appDir);
                string href = virtualAppsRoot() + dirDesc + "/publish.htm";

                htmlString += "<tr>";

                htmlString += "<td>" + dirDesc + "</td>";
                htmlString += "<td style='max-width:250px;'>" + findDescriptionText(dirDesc) + "</td>";
                htmlString += "<td><a href='" + href + "'>Download!</a></td>";

                htmlString += "</tr>";
            }            

            htmlString += "</table></div>";
            return htmlString;
            
        }

        private string findDescriptionText(string appDirName)
        {
            string filePath = physicalAppsRoot() + appDirName + @"\Application Files\" + getLatestAppDir(appDirName) + @"\description.txt.deploy";

            return System.IO.File.Exists(filePath) ?
                System.IO.File.ReadAllText(filePath) :
                string.Empty;
            
        }

        private string getLatestAppDir(string appDirName)
        {
            string rootDir = physicalAppsRoot() + appDirName + @"\Application Files\";
            string latestDir = string.Empty;

            if (System.IO.Directory.Exists(rootDir))
            {
                DateTime dt = System.DateTime.MinValue;

                foreach (string subDir in System.IO.Directory.GetDirectories(rootDir))
                {
                    System.IO.DirectoryInfo dirInf = new System.IO.DirectoryInfo(subDir);
                    if (dirInf.CreationTime > dt)
                    {
                        dt = dirInf.CreationTime;
                        latestDir = subDir;
                    }
 
                }
            }

            return GetDirNameShort(latestDir);
        }

        private string GetDirNameShort(string appDir)
        {
            return appDir.Split('\\')[appDir.Split('\\').Length - 1];
        }

        private string physicalAppsRoot()
        {
            return @"C:\FTP\Eric\Site\Files\Apps\";
        }

        private string virtualAppsRoot()
        {
            return "/Files/Apps/";
        }
    }
}