using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HomeWebApp
{
    public partial class HomePhotos : System.Web.UI.Page
    {

        #region events

        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.MaintainScrollPositionOnPostBack = true;
            btn_next.Focus();

            if (!Page.IsPostBack)
            {
                if (Request.QueryString["album"] != null)
                {
                    string album = Request.QueryString["album"].ToString(); 
                    SetAlbum(album);

                    string pic = Request.QueryString["picture"] != null ? Request.QueryString["picture"].ToString() : Common.GetFirstFileByAlbum(album);
                    SetPicture(pic);

                    if (ShowAllInQueryString()) HttpContext.Current.Session["all"] = GetPictureGridHtml(album, 8);

                    SetCover();                    
                }
            }            
        }

        protected void btn_last_Click(object sender, EventArgs e)
        {
            string[] picFiles = System.IO.Directory.GetFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + CurrentAlbum());
            for (int i = 0; i < picFiles.Length; i++)
            {
                if (System.IO.Path.GetFileName(picFiles[i]) == CurrentPicture())
                {
                    SetPicture(System.IO.Path.GetFileName(picFiles[(i + picFiles.Length - 1) % picFiles.Length]));
                    i = picFiles.Length;
                }
            }
        }

        protected void btn_next_Click(object sender, EventArgs e)
        {
            string[] picFiles = Common.GetPictureFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + CurrentAlbum());
            for (int i = 0; i < picFiles.Length; i++)
            {
                if (System.IO.Path.GetFileName(picFiles[i]) == CurrentPicture())
                {
                    SetPicture(System.IO.Path.GetFileName(picFiles[(i + 1) % picFiles.Length]));
                    i = picFiles.Length;
                }
            }
        }

        protected void btn_all_Click(object sender, EventArgs e)
        {
            string html = GetPictureGridHtml(CurrentAlbum(), 8);

            HttpContext.Current.Session["all"] = html;
            // Common.SendEmail("eric@hackerdevs.com", "eric@hackerdevs.com", System.Web.HttpUtility.HtmlEncode(html), "Test");
        }

        protected void btn_undoAll_Click(object sender, EventArgs e)
        {
            HttpContext.Current.Session["all"] = string.Empty;
        }

        protected void img_currentImage_Click(object sender, ImageClickEventArgs e)
        {            
            Response.Redirect(img_currentImage.ImageUrl.Replace(Common.ALBUM_ROOT_VIRTUAL_DIR_SMALL, Common.ALBUM_ROOT_VIRTUAL_DIR));
        }

        protected void btnSetCurrentPicToCover_Click(object sender, EventArgs e)
        {
            Common.SetPicAlbumCover(CurrentAlbum(), CurrentPicture());
            SetCover();
        }

        #endregion

        #region functions

        void SetCover()
        {
            lblCover.Text = Common.GetFirstFileByAlbum(CurrentAlbum());
            imgCover.ImageUrl = Common.ALBUM_ROOT_VIRTUAL_DIR_SMALL + CurrentAlbum() + "\\" + Common.GetFirstFileByAlbum(CurrentAlbum());
        }

        string CurrentPicture()
        {
            return HttpContext.Current.Session["picture"].ToString();
        }

        void SetPicture(string file)
        {
            HttpContext.Current.Session["picture"] = file;

            if (System.IO.File.Exists(Common.ALBUM_ROOT_PHYSICAL_DIR_SMALL + CurrentAlbum() + "/" + file))
                img_currentImage.ImageUrl = Common.ALBUM_ROOT_VIRTUAL_DIR_SMALL + CurrentAlbum() + "/" + file;
            else
                img_currentImage.ImageUrl = Common.ALBUM_ROOT_VIRTUAL_DIR + CurrentAlbum() + "/" + file;
        }

        string CurrentAlbum()
        {
            return HttpContext.Current.Session["album"].ToString();
        }

        void SetAlbum(string album)
        {
            HttpContext.Current.Session["album"] = album;
        }

        private string GetPictureGridHtml(string album, int columns)
        {
            string html = string.Empty;

            html += "<table border='3'>";
            int count = 0;
            foreach (string file in Common.GetPictureFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + album))
            {
                if (count % columns == 0) html += "<tr>";

                string url = System.IO.File.Exists(Common.ALBUM_ROOT_PHYSICAL_DIR_SMALL + album + "/" + System.IO.Path.GetFileName(file)) ?
                    Common.ALBUM_ROOT_VIRTUAL_DIR_SMALL + album + "/" + System.IO.Path.GetFileName(file) :
                    Common.ALBUM_ROOT_VIRTUAL_DIR + album + "/" + System.IO.Path.GetFileName(file);

                html += "<td><a href='HomePhotos.aspx?picture=" + System.IO.Path.GetFileName(file) + "&album=" + album + "'><img height='75px' src='" + url + "'  /></a></td>";

                if (count % columns == columns - 1) html += "</tr>";
                count++;
            }

            if (!html.EndsWith("</tr>")) html += "</tr>";
            html += "</table>";

            return html;
        }

        private bool ShowAllInQueryString()
        {
            if (Request.QueryString["showall"] == null) return false;
            else return true;
        }

        #endregion
    }
}