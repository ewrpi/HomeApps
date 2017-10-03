using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace HomeWebApp
{
    public partial class AlbumsListing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                gv_albums.RowCommand +=new GridViewCommandEventHandler(gv_albums_RowCommand);
                gv_albums.DataSource = GetAlbumDataSource();
                gv_albums.DataBind();
            }
        }

        protected void gv_albums_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowIndex >= 0)
            {                
                HyperLink hl = (HyperLink)e.Row.Cells[0].Controls[0] as HyperLink;
                hl.NavigateUrl = "~/HomePhotos.aspx?picture=" + Common.GetFirstFileByAlbum(hl.Text) + "&album=" + hl.Text;
                //gv_albums.Rows[e.Row.RowIndex].Cells[0].Text = hl.Text;

                LinkButton btn = (LinkButton)e.Row.Cells[1].Controls[0] as LinkButton;
                btn.Enabled = NoResizedAlbumExists(hl.Text) || NewPictures(hl.Text);
                btn.Text = NoResizedAlbumExists(hl.Text) ? "Yes - begin resizing now" : "No";

            }
        }

        
        private DataTable GetAlbumDataSource()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Albums");
            dt.Columns.Add("image_url");

            foreach (string s in GetAlbums())
            {
                string album = JustDir(s);
                string pic = Common.GetFirstFileByAlbum(album);

                string url = System.IO.File.Exists(Common.ALBUM_ROOT_PHYSICAL_DIR_SMALL + album + "\\" + pic) ?
                    Common.ALBUM_ROOT_VIRTUAL_DIR_SMALL + album + "/" + pic :
                    "~/images/resizeIt.jpg";

                DataRow dr = dt.NewRow();
                dr["Albums"] = album;
                dr["image_url"] = url;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string[] GetAlbums()
        {
            return System.IO.Directory.GetDirectories(Common.ALBUM_ROOT_PHYSICAL_DIR);
        }

        private bool NoResizedAlbumExists(string album)
        {
            return !System.IO.Directory.Exists(Common.ALBUM_ROOT_PHYSICAL_DIR_SMALL + album);
        }

        private bool NewPictures(string album)
        {
            return Common.GetPictureFiles(Common.ALBUM_ROOT_PHYSICAL_DIR + album).Count() != Common.GetPictureFiles(Common.ALBUM_ROOT_PHYSICAL_DIR_SMALL + album).Count();
        }        


        private string JustDir(string dir)
        {
            try { return dir.Split('\\')[dir.Split('\\').Length - 1]; }
            catch { return string.Empty; }
        }

        protected void gv_albums_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "select")
            {
                int index = Convert.ToInt32(e.CommandArgument);

                LinkButton btn = (LinkButton)gv_albums.Rows[index].Cells[1].Controls[0] as LinkButton;
                btn.Enabled = false;

                string album = JustDir(GetAlbums()[index]);                
                string QUOTE = "\"";

                System.Diagnostics.ProcessStartInfo inf = new System.Diagnostics.ProcessStartInfo();
                inf.FileName = ResizeFullPath();
                inf.Arguments = QUOTE + Common.ALBUM_ROOT_PHYSICAL_DIR + album + QUOTE + " " + QUOTE + Common.ALBUM_ROOT_PHYSICAL_DIR_SMALL + album + QUOTE;
                System.Diagnostics.Process.Start(inf);

                lbl_status.Text = "Album {" + album + "} is being resized. You should be able to refresh the page soon and see it no longer needs to be resized.";

            }
        }

        string ResizeFullPath()
        {
            return @"C:\FTP\Eric\Site\resize.exe";
        }


    }
}