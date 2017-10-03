using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using HomeAppsLib;

namespace HomeWebApp
{
    public partial class WinLossStats : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            loadPage();
        }

        private void loadPage()
        {
            this.Title = "Win-Loss Stats by Team";

            string yearQuery = Request.QueryString["yearId"];
            int yearId;
            if (!int.TryParse(yearQuery, out yearId))
                yearId = Common.GetDefaultNFLYearId();
            int sportId = Common.DBModel().NFL_years.First(x => x.id == yearId).sport_id;

            if (!Page.IsPostBack)
                loadDDs(sportId);

            lblTitle.Text = GetLabelTitle(yearId);

            bindGrid(yearId, sportId); 
        }

        private void bindGrid(int yearId, int sportId)
        {
            var data = Common.DBModel();
            System.Data.DataTable gridData = new System.Data.DataTable();
            gridData.Columns.Add("Team");

            int[] weekIds = data.NFL_weeks.Where(x => x.year_id == yearId && (x.weekTypeId == (int)WeekTypes.RegularSeason || x.weekTypeId == (int)WeekTypes.PostSeason)).Select(x => x.week).ToArray();
            int[] weekIdsWithStats = data.NFL_Matchups.Where(x => x.winner != null && weekIds.Contains(x.week)).Select(x => x.week).ToArray();
            //int count = 0;
            foreach (var wk in data.NFL_weeks.Where(x => weekIdsWithStats.Contains(x.week)))
            {
                //count++;
                gridData.Columns.Add(wk.text);
            }

            string[] onlyTeams = getTeamFilterArray();
            List<db.NFL_team> allTeams = data.NFL_teams.Where(x => x.sport_id == sportId && x.display_flag && (onlyTeams.Length == 0 || onlyTeams.Contains(x.name))).OrderBy(x => x.name).ToList();
            //allTeams.AddRange(Common.DBModel().NFL_teams.Where(x => x.name == "AFC" || x.name == "NFC").ToList()); // want this at the bottom
            foreach (var team in allTeams)
            {
                System.Data.DataRow dr = gridData.NewRow();

                dr["Team"] = team.name;

                foreach (var wk in data.NFL_weeks.Where(x => weekIdsWithStats.Contains(x.week)))
                {
                    var matchup = data.NFL_Matchups.FirstOrDefault(x => x.week == wk.week && (x.home == team.name || x.away == team.name) && x.winner != null);

                    if (matchup != null)
                    {
                        string text = string.Empty;

                        if (matchup.winner == team.name) // WON
                            text += "Won against ";
                        else if (matchup.winner == "TIE")
                            text += "Tied the ";
                        else
                            text += "Lost to ";

                        if (matchup.home == team.name)
                            text += "<b>" + matchup.away + "</b><hr>Home";
                        else
                            text += "<b>" + matchup.home + "</b><hr>Away";

                        text += "<hr>";

                        bool isHomeTeam = matchup.home == team.name;
                        int? score = isHomeTeam ? matchup.home_score : matchup.away_score;
                        int? oppScore = isHomeTeam ? matchup.away_score : matchup.home_score;
                        text += score + " to " + oppScore;

                        dr[matchup.NFL_week.text] = text;
                    }
                    //else dr[i] = "matchup == null";
                }

                gridData.Rows.Add(dr);
            }

            gvWinLossStats.DataSource = gridData;
            gvWinLossStats.DataBind();

            foreach (GridViewRow row in gvWinLossStats.Rows)
            {
                int winCount = 0, lossCount = 0, tieCount = 0;
                foreach (TableCell cell in row.Cells)
                {
                    if (!string.IsNullOrEmpty(cell.Text))
                    {
                        cell.Attributes.Add("style", "border:solid;border-color:Black;");
                        string decodedText = System.Web.HttpUtility.HtmlDecode(cell.Text);
                        cell.Text = decodedText;
                    }

                    if (cell.Text.StartsWith("Won"))
                    {
                        cell.BackColor = System.Drawing.Color.Blue;
                        cell.ForeColor = System.Drawing.Color.Yellow;
                        winCount++;
                    }
                    else if (cell.Text.StartsWith("Lost"))
                    {
                        cell.BackColor = System.Drawing.Color.Red;
                        lossCount++;
                    }
                    else if (cell.Text.StartsWith("Tied"))
                    {
                        cell.BackColor = System.Drawing.Color.LightGreen;
                        tieCount++;
                    }
                }

                row.Cells[0].Text += "<br>" + winCount + "-" + lossCount;
                if (tieCount > 0) row.Cells[0].Text += "-" + tieCount;
            }
        }

        private void loadDDs(int sportId)
        {
            var data = Common.DBModel().NFL_teams.Where(t => t.sport_id == sportId).Select(t => new { Value = t.name, Text = t.name }).OrderBy(i => i.Text);

            ddlTeam1.DataSource = data;
            ddlTeam1.DataValueField = "Value";
            ddlTeam1.DataTextField = "Text";
            ddlTeam1.DataBind();

            ddlTeam2.DataSource = data;
            ddlTeam2.DataValueField = "Value";
            ddlTeam2.DataTextField = "Text";
            ddlTeam2.DataBind();
        }

        private string[] getTeamFilterArray()
        {
            if (HttpContext.Current.Session["WinLossStatsTeamFilter"] != null)
                return (string[])HttpContext.Current.Session["WinLossStatsTeamFilter"];
            else
                return new string[0];
        }
        private void setTeamFilterArray(string[] array)
        {
            HttpContext.Current.Session["WinLossStatsTeamFilter"] = array;             
        }

        private string GetLabelTitle(int yearId)
        {
            var year = Common.DBModel().NFL_years.First(x => x.id == yearId);
            var sport = Common.DBModel().NFL_sports.First(x => x.id == year.sport_id);

            return sport.name + " " + year.text;
        }

        protected void btnSumit_Click(object sender, EventArgs e)
        {
            string team1 = ddlTeam1.SelectedValue.ToString();
            string team2 = ddlTeam2.SelectedValue.ToString();
            setTeamFilterArray(new string[] { team1, team2 });
            loadPage();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            setTeamFilterArray(new string[0]);
            loadPage();
        }
    }
}