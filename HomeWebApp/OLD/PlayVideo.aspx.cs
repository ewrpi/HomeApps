using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class PlayVideo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string videoPath;
                if (Request.QueryString["video"] == null)
                {
                    SetVideoPath("No video selected."); // TODO: perhaps create a default video.
                    btn_play.Enabled = false;
                }
                else
                {
                    videoPath = Common.VIDEO_ROOT_PHYSICAL_DIR + Request.QueryString["video"].ToString();
                    SetVideoPath(videoPath);
                    btn_play.Enabled = true;
                }

                lbl_videoPath.Text = GetVideoPath();
            }

            else
                SetVideoPath(GetVideoPath());
        }

        private void StreamVideo()
        {
            HttpContext context = HttpContext.Current;
            string localPath = GetVideoPath();

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(localPath);

            int len = (int)fileInfo.Length;
            context.Response.AppendHeader("content-length", len.ToString());
            context.Response.ContentType = "video/mp4";

            // stream file
            byte[] buffer = new byte[1 << 16]; // 64kb
            int bytesRead = 0;
            using (var file = System.IO.File.Open(localPath, System.IO.FileMode.Open))
            {
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) != 0)
                {
                    context.Response.OutputStream.Write(buffer, 0, bytesRead);
                }
            }

            // finish
            context.Response.Flush();
            context.Response.Close();
            context.Response.End(); 

        }

        //private void StreamVideo()
        //{
        //    System.IO.FileStream fs = new System.IO.FileStream(GetVideoPath(), System.IO.FileMode.Open, System.IO.FileAccess.Read);
        //    //System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

        //    byte[] buffer = new byte[4096];

        //    while (true)
        //    {
        //        int bytesRead = fs.Read(buffer, 0, buffer.Length);
        //        if (bytesRead == 0) break;
                                
        //        Response.ContentType = "video/mp4";
        //        Response.OutputStream.Write(buffer, 0, bytesRead);
        //    }
        //}

        private void SetVideoPath(string videoPath)
        {
            HttpContext.Current.Session["videoPath"] = videoPath;
        }

        private string GetVideoPath()
        {
            return HttpContext.Current.Session["videoPath"].ToString();
        }

        protected void btn_play_Click(object sender, EventArgs e)
        {
            StreamVideo();
        }
    }
}