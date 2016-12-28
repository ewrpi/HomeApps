using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using HomeAppsLib;

namespace HomeWebApp
{
    public partial class NFLMakePicksMobile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Meta.LogRequest(Request.ServerVariables["remote_addr"].ToString(), Request.RawUrl, getControlDesc());
            string mainPage = "/NFLPicks.aspx";
            string thisPage = "/NFLMakePicksMobile.aspx";

            if (Authentication.IsUserAuthenticated())
            {
                int weekInt = GetWeek();
                var week = Common.DBModel().NFL_weeks.First(x => x.week == weekInt);

                this.Title = week.text;
                lblWeekText.Text = week.text;

                if (!Common.HasUserMadePicksForWeek(weekInt) && (week.exp_dt > DateTime.Now || Common.CurrentUser.IsKid))
                {
                    string[] picks = GetPicks().Split(',');

                    var matchups = Common.DBModel().NFL_Matchups
                        .Where(x => x.week == weekInt)
                        .Where(x => !picks.Contains(x.home) && !picks.Contains(x.away)).ToList();

                    if (matchups.Count > 0)
                    {
                        var matchup = matchups.First();

                        lblHomeName.Text = matchup.home;
                        lblVisitorName.Text = matchup.away;

                        lblHomeCity.Text = Common.DBModel().NFL_teams.First(x => x.name == matchup.home).location;
                        lblVisitorCity.Text = Common.DBModel().NFL_teams.First(x => x.name == matchup.away).location;

                        imgHome.ImageUrl = "/images/teamlogos/" + matchup.home + ".png";
                        //imgHome.Width = new Unit(100);
                        imgVisitor.ImageUrl = "/images/teamlogos/" + matchup.away + ".png";
                        //imgVisitor.Width = new Unit(100);

                        AddHidden("home", matchup.home);
                        AddHidden("visitor", matchup.away);
                        AddHidden("autoruncolors", Common.CurrentUser.IsKid.ToString());
                        AddHidden("floatImages", GetCommaDelimitedFloatImages());
                    }
                    else if (picks.Length == week.games)
                    {
                        Common.SubmitUserPicks(weekInt, picks);
                        Common.DeleteUserSetting("PICKS-WEEK-" + weekInt);

                        int? nextWeekInt = Common.GetNextWeek(weekInt);
                        if (nextWeekInt.HasValue)
                            Response.Redirect(thisPage + "?week=" + nextWeekInt.Value);
                        else
                            Response.Redirect(mainPage);
                    }
                }
                else Response.Redirect(mainPage);
            }
            else
                Response.Redirect(mainPage);
        }

        private string GetCommaDelimitedFloatImages()
        {
            List<string> resultList = new List<string>();

            foreach (string filePath in System.IO.Directory.GetFiles(Server.MapPath("/images/floatImages")))
                resultList.Add(System.IO.Path.GetFileName(filePath));

            return string.Join(",", resultList.ToArray());
        }        

        private void AddHidden(string key, string value)
        {
            System.Web.UI.HtmlControls.HtmlGenericControl hidden = new System.Web.UI.HtmlControls.HtmlGenericControl("input");
            hidden.Attributes.Add("type", "hidden");
            hidden.Attributes.Add("id", key);
            hidden.Attributes.Add("value", value);
            pnlHidden.Controls.Add(hidden);
        }

        private string GetPicks()
        {
            string picks = Common.GetUserSetting("PICKS-WEEK-" + GetWeek(), string.Empty);

            if (Request.QueryString["choice"] != null && !picks.Contains(Request.QueryString["choice"].ToString()))
            {
                if (picks.Length == 0)
                    picks = Request.QueryString["choice"].ToString();
                else
                {
                    // need to see if their opponent already exists and remove in case they hit BACK in the browser.
                    int weekInt = GetWeek();
                    string choice = Request.QueryString["choice"].ToString();
                    var matchup = Common.DBModel().NFL_Matchups.First(x => (x.home == choice || x.away == choice) && x.week == weekInt);

                    List<string> picksList = picks.Split(',').ToList();
                    if (picksList.Contains(matchup.away) && matchup.home == choice)
                    {
                        // remove matchup.away                         
                        picksList.Remove(matchup.away);
                    }
                    if (picksList.Contains(matchup.home) && matchup.away == choice)
                    {
                        // remove matchup.home                        
                        picksList.Remove(matchup.home);
                    }

                    picksList.Add(choice);
                    picks = string.Join(",", picksList.ToArray());
                }
            }

            Common.SetUserSetting("PICKS-WEEK-" + GetWeek(), picks);

            return picks;
        }
                

        private int GetWeek()
        {
            if (Request.QueryString["week"] == null)
            {
                if (Session["autoweek"] == null)
                    Session["autoweek"] = Common.GetDefaultWeek(toMakePicks: true);
                
                return Convert.ToInt32(Session["autoweek"]);
            }
            else
            {
                int week = Convert.ToInt32(Request.QueryString["week"]);
                Session["autoweek"] = week;
                return week;
            }

        }

        private string getControlDesc()
        {
            string team = Request.QueryString["choice"] ?? "Initial Load";
            string homeAway = Request.QueryString["homeOrVisitor"] ?? "Initial Load";

            return "Team {" + team + "}, HomeOrVisitor {" + homeAway + "}, Week {" + GetWeek() +"}";
        }

        protected void lnkCancel_Click(object sender, EventArgs e)
        {
            Common.DeleteUserSetting("PICKS-WEEK-" + GetWeek(), softDelete: true);
            Response.Redirect("/NFLPicks.aspx");
        }

        protected void lnkLogOut_Click(object sender, EventArgs e)
        {
            Authentication.LogUserOut();
            Response.Redirect("~/NFLPicks.aspx");
        }
    }
}