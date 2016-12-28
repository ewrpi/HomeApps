using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using HomeAppsLib;

namespace HomeWebApp
{
    public partial class NFLPicks : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.MaintainScrollPositionOnPostBack = true;
            int week;

            // use cookie to keep user authenticated
            if (!Authentication.IsUserAuthenticated() && Request.Cookies[Authentication.AUTHENTICATED_USER_STRING] != null && Request.Cookies[Authentication.AUTHENTICATED_USER_ENC_PASSWORD_STRING] != null) 
            {
                Authentication.ValidateUser(Request.Cookies[Authentication.AUTHENTICATED_USER_STRING].Value, Request.Cookies[Authentication.AUTHENTICATED_USER_ENC_PASSWORD_STRING].Value, true); 
            }
            if (!Authentication.IsUserAuthenticated() && Request.QueryString["sso"] != null)
            {
                Authentication.ValidateUserSSO(Request.QueryString["sso"].ToString());
            }

            // hard redirect for changing week and handle week default.
            if (Page.IsPostBack && Common.FindWhatControlDidPostBack(this.Page).ID == "ddlWeeks") // to fix exception being thrown because of dynamic controls and postbacking: // An error has occurred because a control with id 'ctl00$ContentPlaceHolder1$ctl02' could not be located or a different control is assigned to the same ID after postback. If the ID is not assigned, explicitly set the ID property of controls that raise postback events to avoid this error.
            {
                Common.SetLastVisitedWeek(Convert.ToInt32(ddlWeeks.SelectedValue));
                Meta.LogRequest(Request.ServerVariables["remote_addr"].ToString(), Request.RawUrl, "System.Web.UI.WebControls.DropDownList ddlWeeks (week " + ddlWeeks.SelectedValue + ")");
                Response.Redirect(Request.RawUrl);
            }
            else if (Request.QueryString["autoweek"] != null && int.TryParse(Request.QueryString["autoweek"].ToString(), out week))
            {
                HttpContext.Current.Session["autoweek"] = week.ToString();
                Common.SetLastVisitedWeek(week);
            }
            else
                HttpContext.Current.Session["autoweek"] = Common.GetLastWeekVisited();
            
            bool auth = Authentication.IsUserAuthenticated();

            pnlLoggedIn.Visible = auth;
            pnlLogInOrRegister.Visible = !auth;

            //this.Title = "NFLPicks" + (auth ? " - " + Authentication.GetAuthenticatedUserName() : "");
            if (auth)
                this.Title = "The Wright Picks | " + Authentication.GetAuthenticatedUserName();

            if (!Page.IsPostBack)
            {
                LoadDDs();
                initializeOtherControls();
            }
            
            if (auth)
                loadPage();
        }

        private void initializeOtherControls()
        {
            btnAddThisUser.Visible = false;
            ddlUsersThatCanByAdded.Visible = false;
            btnResetAddUsers.Visible = btnAddTheBetUsers.Visible = false;// Authentication.IsABetUser(GetCurrentYearId());


            // by default exclude preseason from cumm stats if current year has regular season stats
            int yearId = GetCurrentYearId();
            var data = Common.DBModel();
            int[] playedRegOrPostWeeks = data.NFL_weeks.Where(x => x.exp_dt < DateTime.Now && x.year_id == yearId && (x.weekTypeId == (int)WeekTypes.PostSeason || x.weekTypeId == (int)WeekTypes.RegularSeason)).Select(x => x.week).ToArray();
            int regSeasonStatCount = data.NFL_userPicks.Count(x => playedRegOrPostWeeks.Contains(x.week));
            if (regSeasonStatCount > 0)
                cbCummStatsPre.Checked = false;

            Common.AutoLoadAdminPanel = false;

            lblAdminStatus.Text = displayCurrentMatchupsForAdminPanel(Convert.ToInt32(ddlWeeksAdmin.SelectedValue));

            cblEmailSubscriptions.DataSource = Common.DBModel().emailSubscriptionTypes;
            cblEmailSubscriptions.DataTextField = "Name";
            cblEmailSubscriptions.DataValueField = "Id";
            cblEmailSubscriptions.DataBind();

            foreach (ListItem cb in cblEmailSubscriptions.Items)
                cb.Selected = HomeAppsLib.EmailSubscriptions.UserHasSubscription(Convert.ToInt32(cb.Value), Authentication.GetAuthenticatedUserName());
        }

        //class TimeLogItem { public DateTime Time { get; set; } public string Text { get; set; } }
        private void loadPage()
        {
            populateNFLSiteData();

            int week = Convert.ToInt32(ddlWeeks.SelectedValue);
            Session["autoweek"] = week.ToString();

            if (Common.HasUserMadePicksForWeek(week) || (Common.ArePicksClosedForWeek(week) && !Common.CurrentUser.IsKid))
            {
                System.Web.UI.HtmlControls.HtmlGenericControl hidden = new System.Web.UI.HtmlControls.HtmlGenericControl("input");
                hidden.Attributes.Add("type", "hidden");
                hidden.Attributes.Add("name", "picksMade");
                pnlCrazyArrows.Controls.Add(hidden); // need all this for those crazy arrows

                pnlStats.Visible = true;
                pnlMakePicks.Visible = false;
                
                loadAllDataGrids();
                buildForum();

                lblPromptToChooseWeek.Text = string.Empty;
            }

            else if (week < 0)
            {
                pnlStats.Visible = false;
                pnlMakePicks.Visible = false;
                lblPromptToChooseWeek.Text = "Choose a week from the drop down menu.";
            }

            else
            {
                lblPromptToChooseWeek.Text = string.Empty;                
                pnlStats.Visible = false;
                pnlMakePicks.Visible = true;

                DateTime pickExpDt = Common.DBModel().NFL_weeks.First(x => x.week == week).exp_dt;

                if (DateTime.Now < pickExpDt)
                {
                    lblExpiration.Text = "Expire " + pickExpDt.ToShortDateString() + " " + pickExpDt.ToShortTimeString() + "!!";
                    pnlMakePicksControls.Visible = true;
                    lblPicksError.Text = string.Empty;
                    btnSubmitPicks.Visible = true;
                    buildPicksGrid(week);                    
                }
                else if (Common.CurrentUser.IsKid)
                {
                    Response.Redirect("/NFLMakePicksMobile.aspx");
                }
                else
                {
                    pnlMakePicksControls.Visible = false;
                    lblPicksError.Text = "Picks have expired for this week. <br><a href='Email.aspx?m=PLEAD YOUR CASE!!!'>But I really want to!!</a> ";
                    btnSubmitPicks.Visible = false;
                }

            }
        }        

        private void buildPicksGrid(int week)
        {
            pnlPicksGrid.Controls.Clear();

            System.Web.UI.WebControls.Table table = new Table();
            table.BackColor = System.Drawing.Color.FromArgb(247, 247, 222);
            table.BorderColor = System.Drawing.Color.DarkGreen;
            table.BorderStyle = BorderStyle.Solid;
            table.BorderWidth = new Unit(5);
            table.Attributes.Add("border", "1");

            int count = 0;
            foreach (string[] match in Common.GetMatchups(week))
            {
                TableRow row = BuildTableRow(match[0], match[1], count, week);
                table.Controls.Add(row);
                count++;
            }

            pnlPicksGrid.Controls.Add(table);
        }

        private TableRow BuildTableRow(string awayTeam, string homeTeam, int count, int week)
        {
            System.Web.UI.WebControls.TableRow row = new TableRow();

            int sportId = Common.GetCurrentSportId();

            string[] savedPicks = Common.GetUserSetting("PICKS-WEEK-" + week, string.Empty).Split(',');

            TableCell[] cells = new TableCell[4];
            for (int i = 0; i < 4; i++) cells[i] = new TableCell();

            System.Web.UI.WebControls.Label visLabel = new Label();
            visLabel.Text = Common.DBModel().NFL_teams.First(x => x.name == awayTeam && x.sport_id == sportId).location + " " + awayTeam + " ";
            cells[0].Attributes.Add("align", "right");
            cells[0].Controls.Add(visLabel);

            string quickpicksvalue = "";
            if (savedPicks.Length == 1 && savedPicks.First() == string.Empty && Request.QueryString["quickpicks"] == "true")
            {
                quickpicksvalue = new Random(DateTime.Now.Millisecond).NextDouble() < 0.5 ? "home" : "away";
            }

            System.Web.UI.HtmlControls.HtmlInputRadioButton away = new System.Web.UI.HtmlControls.HtmlInputRadioButton();
            away.Name = count.ToString();
            away.Value = awayTeam;
            away.Checked = savedPicks.Contains(awayTeam) || quickpicksvalue == "away";
            cells[1].Controls.Add(away);

            System.Web.UI.HtmlControls.HtmlInputRadioButton home = new System.Web.UI.HtmlControls.HtmlInputRadioButton();
            home.Name = count.ToString();
            home.Value = homeTeam;
            home.Checked = savedPicks.Contains(homeTeam) || quickpicksvalue == "home";
            cells[2].Attributes.Add("align", "left");
            cells[2].Controls.Add(home);

            Label homeLabel = new Label();
            homeLabel.Text = " " + Common.DBModel().NFL_teams.First(x => x.name == homeTeam && x.sport_id == sportId).location + " " + homeTeam;
            cells[3].Controls.Add(homeLabel);

            for (int i = 0; i < 4; i++)
                row.Controls.Add(cells[i]);

            return row;
        }

        private void populateNFLSiteData()
        {
            try
            {
                System.Web.UI.HtmlControls.HtmlGenericControl div = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                System.Web.UI.HtmlControls.HtmlGenericControl div2 = new System.Web.UI.HtmlControls.HtmlGenericControl("div");                

                div.InnerHtml = GetNFLSiteData("http://www.nfl.com/standings?category=league", "NFLSiteDataLeague");
                div2.InnerHtml = GetNFLSiteData("http://www.nfl.com/standings?category=division", "NFLSiteDataDivision");

                pnlLeagueStandings.Controls.Add(div);
                pnlLeagueStandings.BackColor = System.Drawing.Color.FromArgb(247, 247, 222);

                pnlDivisionStandings.Controls.Add(div2);
                pnlDivisionStandings.BackColor = System.Drawing.Color.FromArgb(247, 247, 222);
            }
            catch { }
        }

        private string GetNFLSiteData(string url, string key)
        {
            System.Net.WebClient wc = new System.Net.WebClient();

            if (Session[key] == null)
                Session[key] = ParseAndFormatNFLSiteStandingsData(wc.DownloadString(url));

            return Session[key].ToString();
        }

        private string ParseAndFormatNFLSiteStandingsData(string fullHTML)
        {
            string html = fullHTML.Substring(fullHTML.IndexOf("<table"));
            html = html.Substring(0, html.IndexOf("</table>") + 8);
            return html.Replace("href=\"/", "target=\"_blank\" href=\"http://www.nfl.com/").Replace("<table ", "<table border='1' style='padding:10px;' ");
        }
        

        private void buildForum()
        {
            pnlForumControls.Visible = true;
            pnlForum.Controls.Clear();      

            System.Web.UI.HtmlControls.HtmlGenericControl div = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
            int selectedWeek = Convert.ToInt32(ddlWeeks.SelectedValue);

            var topics = LibCommon.DBModel().NFL_forums.Where(x => x.week == selectedWeek && x.ref_id == null).OrderByDescending(i => i.insert_dt);

            int count = 0;
            foreach (var item in topics)
            {
                count++;

                System.Web.UI.HtmlControls.HtmlGenericControl innerDiv = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                innerDiv.Attributes.Add("class", "forumTopic");
                innerDiv.Attributes.Add("id", "forumTopic-" + item.id);

                if (count == 1)
                {
                    System.Web.UI.HtmlControls.HtmlGenericControl globalJavaScriptControlDiv = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                    globalJavaScriptControlDiv.Attributes.Add("class", "forumTopic");

                    System.Web.UI.HtmlControls.HtmlGenericControl showUser = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                    showUser.Attributes.Add("onclick", "showUser()");
                    showUser.Attributes.Add("align", "left");
                    showUser.Attributes.Add("class", "blueLink");
                    showUser.InnerHtml = "Expand topics by " + getDDForUsers() + "<br>";

                    System.Web.UI.HtmlControls.HtmlGenericControl showAll = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                    showAll.Attributes.Add("onclick", "showAll()");
                    showAll.Attributes.Add("align", "left");
                    showAll.Attributes.Add("class", "blueLink");
                    showAll.InnerText = "Expand All";

                    System.Web.UI.HtmlControls.HtmlGenericControl hideAll = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                    hideAll.Attributes.Add("onclick", "hideAll()");
                    hideAll.Attributes.Add("align", "left");
                    hideAll.Attributes.Add("class", "blueLink");
                    hideAll.InnerText = "Collapse All";

                    globalJavaScriptControlDiv.Controls.Add(showUser);                    
                    globalJavaScriptControlDiv.Controls.Add(showAll);                    
                    globalJavaScriptControlDiv.Controls.Add(hideAll);

                    div.Controls.Add(globalJavaScriptControlDiv);
                }

                System.Web.UI.HtmlControls.HtmlGenericControl header = new System.Web.UI.HtmlControls.HtmlGenericControl("h3");
                header.InnerHtml = "<b>" + item.title + "</b> by " + item.username;// +": " + item.comment.Substring(0, Math.Min(20, item.comment.Length)) + "...<br>Participants: " + Common.GetDiscussionParticipantsById(item.id);
                innerDiv.Controls.Add(header);

                System.Web.UI.HtmlControls.HtmlGenericControl divToShowHide = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                divToShowHide.Attributes.Add("onclick", "showHide(" + item.id + ")");
                divToShowHide.Attributes.Add("class", "blueLink");                
                divToShowHide.InnerText = "Expand or Collapse";
                divToShowHide.Controls.Add(new System.Web.UI.HtmlControls.HtmlGenericControl("br"));

                System.Web.UI.HtmlControls.HtmlGenericControl divWithBlockQuote = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                string display = IsPostDisplayed(item, count == 1) ? "block" : "none"; // <<FINISHED>>TODO: think of how to initialize this to either "none" or "block"
                HttpContext.Current.Session["defaultTopicId"] = null;
                divWithBlockQuote.Attributes.Add("id", item.id.ToString());
                divWithBlockQuote.Attributes.Add("name", "discussionPosts");
                divWithBlockQuote.Attributes.Add("person", item.username);
                divWithBlockQuote.Attributes.Add("style", "display:" + display + ";");
                divWithBlockQuote.Controls.Add(GetBlockQuoteControl(item.id));                

                innerDiv.Controls.Add(divToShowHide);
                innerDiv.Controls.Add(divWithBlockQuote);

                div.Controls.Add(innerDiv);
            }

            if (count == 0)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl innerDiv = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                innerDiv.Attributes.Add("class", "forumTopic");

                innerDiv.InnerText = "No comments have been posted yet... be the first!!";
                innerDiv.Attributes.Add("height", "100px");
                div.Controls.Add(innerDiv);
            }

            pnlForum.Controls.Add(div);
        }

        private bool IsPostDisplayed(HomeAppsLib.db.NFL_forum post, bool appearsOnTop)
        {
            return ((HttpContext.Current.Session["defaultTopicId"] != null) 
                        && (HttpContext.Current.Session["defaultTopicId"].ToString() == post.id.ToString()))
                || post.insert_dt.Value > DateTime.Now.AddDays(-1)
                || appearsOnTop
                || Common.MaxReplyDate(post) > DateTime.Now.AddDays(-1);

        }

        private string getDDForUsers()
        {
            string html = "<select id='userDD'>";
            int week = Convert.ToInt32(ddlWeeks.SelectedValue);

            foreach (string user in LibCommon.DBModel().NFL_forums.Where(x => x.week == week && x.ref_id == null).Select(x => x.username).Distinct())
            {
                html += "<option value='" + user + "'>" + user + "</option>";
            }

            html += "</select>";
            return html;
        }

        private Control GetBlockQuoteControl(int id)
        {
            System.Web.UI.HtmlControls.HtmlGenericControl blockQuote = new System.Web.UI.HtmlControls.HtmlGenericControl("blockquote");

            var dbComment = LibCommon.DBModel().NFL_forums.First(x => x.id == id);
            blockQuote.InnerHtml = "<div style='border:solid;padding:10px;border-width:1px;'><div><b>" + dbComment.username + " @ " + FormatDateForUI(dbComment.insert_dt.Value) + "</b></div><div>" + dbComment.DisplayComment + "</div></div>";

            System.Web.UI.WebControls.LinkButton reply = new LinkButton();
            reply.Text = "Reply";            
            reply.CommandArgument = id.ToString();
            reply.OnClientClick = "setGlobalWait()";
            reply.Command += new CommandEventHandler(reply_Command);

            blockQuote.Controls.Add(reply);

            foreach (var item in LibCommon.DBModel().NFL_forums.Where(x => x.ref_id == id))
                blockQuote.Controls.Add(GetBlockQuoteControl(item.id));
            
            return blockQuote;
        }

        private string FormatDateForUI(DateTime dateTime)
        {
            return dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString();
        }

        void reply_Command(object sender, CommandEventArgs e)
        {
            btnSubmitComment.CommandArgument = e.CommandArgument.ToString();
            btnSubmitComment.Visible = true;
            txtComment.Visible = true;
            cbEmailOnReply.Visible = cbEmailOnReply.Checked = Common.CurrentUser.email != null;
            btnPostNew.Visible = false;
            txtCommentTitle.Visible = false;
            lblStaticPromptForCommentTitle.Visible = false;

            this.MaintainScrollPositionOnPostBack = false;            
            SetFocus(txtComment);

            int id = Convert.ToInt32(e.CommandArgument);            
            var replyingToComment = LibCommon.DBModel().NFL_forums.First(x => x.id == id);            

            lblReplyingTo.Text = "Replying to " + replyingToComment.username + " on topic \"" + Common.FindTopicTitle(id) + "\"";

            
        }

        private void LoadDDs()
        {
            // For all users
            int sportId = Common.GetCurrentSportId();
            ddlSports.DataSource = Common.DBModel().NFL_sports;
            ddlSports.DataTextField = "name";
            ddlSports.DataValueField = "id";
            ddlSports.SelectedValue = sportId.ToString();
            ddlSports.DataBind();

            ddlYears.DataSource = Common.DBModel().NFL_years.Where(x => x.sport_id == sportId);
            ddlYears.DataTextField = "text";
            ddlYears.DataValueField = "id";
            ddlYears.SelectedValue = GetCurrentYearId().ToString();
            ddlYears.DataBind();

            int yearId = GetCurrentYearId();
            ddlWeeks.DataSource = Common.DBModel().NFL_weeks.Where(x => x.year_id == yearId && x.display_flag).OrderBy(x => x.sort_id).ThenBy(x => x.year_week);
            ddlWeeks.DataValueField = "week";
            ddlWeeks.DataTextField = "text";
            ddlWeeks.DataBind();

            if (HttpContext.Current.Session["autoweek"] != null)
            {
                int weekId = Convert.ToInt32(HttpContext.Current.Session["autoweek"]);
                ddlWeeks.SelectedValue = weekId.ToString();
                HttpContext.Current.Session["autoweek"] = null;

                loadUserPicksToAddToMatchupsGrid(weekId);
            }

            ddlWeeksAdmin.DataSource = Common.DBModel().NFL_weeks.Where(x => x.year_id == yearId && x.week >= 0).OrderBy(x => x.sort_id).ThenBy(x => x.year_week);
            ddlWeeksAdmin.DataValueField = "week";
            ddlWeeksAdmin.DataTextField = "text";
            ddlWeeksAdmin.DataBind();
            ddlWeeksAdmin.SelectedValue = ddlWeeks.SelectedValue;

            if (pnlLogInOrRegister.Visible)
            {
                ddlStayLoggedOn.Items.Add(new ListItem("No", DateTime.Now.ToString()));
                ddlStayLoggedOn.Items.Add(new ListItem("Yes - 1 day", DateTime.Now.AddDays(1).ToString()));
                ddlStayLoggedOn.Items.Add(new ListItem("Yes - 1 week", DateTime.Now.AddDays(7).ToString()));
                ddlStayLoggedOn.Items.Add(new ListItem("Yes - Forever", DateTime.Now.AddYears(100).ToString()));
            }

            ddlCummStatGrouping.Items.Add("25");
            ddlCummStatGrouping.Items.Add("50");
            ddlCummStatGrouping.Items.Add("75");
            ddlCummStatGrouping.Items.Add("100");
            ddlCummStatGrouping.SelectedValue = Common.GetCummalitveStatsGrouping();

            // Admin Only
            if (Authentication.AdminUsers().Contains(Authentication.GetAuthenticatedUserName()))
            {
                ddlVis.DataSource = ddlHome.DataSource = Common.DBModel().NFL_teams.Where(x => x.sport_id == sportId).OrderBy(x => x.name).Select(y => y.name);
                ddlVis.DataBind();
                ddlHome.DataBind();

                //ddlGameTypes.DataSource = Common.DBModel().NFL_GameTypes;
                //ddlGameTypes.DataValueField = "id";
                //ddlGameTypes.DataTextField = "name";
                //ddlGameTypes.SelectedValue = "1";
                //ddlGameTypes.DataBind();

                //ddlDivisions.DataSource = Common.DBModel().NFL_divisions.Select(x => x.description);
                //ddlDivisions.DataBind();
            }         
        }

        private void loadUserPicksToAddToMatchupsGrid(int weekId)
        {
            string[] users = Common.DBModel().NFL_userPicks.Where(x => x.week == weekId).Select(x => x.username).ToArray();
            string[] usersAlreadyAdded = new string[0];
            if (Common.getUsersToAddToMatchupsGrid() != null)
                usersAlreadyAdded = Common.getUsersToAddToMatchupsGrid();
            foreach (string username in users.Where(u => !usersAlreadyAdded.Contains(u)))
            {
                ddlUsersThatCanByAdded.Items.Add(new ListItem { Value = username, Text = username });
            }
        }

        private void loadAllDataGrids()
        {
            var data = Common.DBModel();
            int week = Convert.ToInt32(ddlWeeks.SelectedValue);
            int yearId = GetCurrentYearId();

            loadWeeklyStats(data, week);
            loadMatchups(data, week);

            if (Convert.ToBoolean(ddlCummStatsRemoveYearFilter.SelectedValue))
                loadCummulativeStats(data, week);
            else
                loadCummulativeStats(data, week, yearId);

            if (Authentication.IsABetUser(GetCurrentYearId()))
            {
                pnlTheBet.Visible = true;
                gvTheBet.DataSource = GetTheBetDataSource();
                gvTheBet.DataBind();
                HtmlDecode(gvTheBet);
            }
            else
                pnlTheBet.Visible = false;
        }

        private void HtmlDecode(GridView grid)
        {
            foreach(GridViewRow row in grid.Rows)
                foreach (TableCell cell in row.Cells)
                    cell.Text = System.Web.HttpUtility.HtmlDecode(cell.Text);
        }

        private object GetTheBetDataSource()
        {
            int yearId = GetCurrentYearId();

            System.Data.DataTable dt = CacheHelper.GetCachedGridViewTheBetDataSource(yearId).Copy();

            System.Data.DataView view = dt.DefaultView;
            view.Sort = "Money DESC";
            dt.Columns.Remove("Money"); // for sorting purposes only
            
            return view;
        }
        
        private void loadCummulativeStats(db.DBModelDataContext data, int week, int? yearId = null)
        {
            List<int> weekTypes = new List<int>();
            if (cbCummStatsPre.Checked)
                weekTypes.Add((int)WeekTypes.PreSeason);
            if (cbCummStatsRegular.Checked)
                weekTypes.Add((int)WeekTypes.RegularSeason);
            if (cbCummStatsPost.Checked)
                weekTypes.Add((int)WeekTypes.PostSeason);

            System.Data.DataTable allStats = CacheHelper.GetCachedCummmulativeGridViewStatsDataSource(week, yearId, weekTypes.ToArray());
            
            var obj = ConvertToCummulativeGridObjects(allStats);
            System.Web.UI.HtmlControls.HtmlGenericControl table = new System.Web.UI.HtmlControls.HtmlGenericControl("table");
            System.Web.UI.HtmlControls.HtmlGenericControl tr = new System.Web.UI.HtmlControls.HtmlGenericControl("tr");
            int lessThan = 0;
            int increment = Convert.ToInt32(ddlCummStatGrouping.SelectedValue);
            int maxVal = Common.GetTotalNumberOfNFLMatchups();
            while (lessThan < maxVal) lessThan += increment;
            int numberOfGridsAdded = 0;

            do
            {
                List<CummulativeStatsGridObject> dataSource = obj.Where(x => Convert.ToInt32(x.Ratio.Split('/')[1]) >= lessThan - increment && Convert.ToInt32(x.Ratio.Split('/')[1]) < lessThan).OrderByDescending(x => x.Percent).ToList();

                if (dataSource.Count() > 0)
                {
                    numberOfGridsAdded++;
                    GridView gv = CreateCummulativeStatsGrid();

                    if (numberOfGridsAdded == 1)
                    {
                        List<CummulativeStatsGridObjectWithRanking> dsWithRanking = dataSource.Select(x => x.ToGridWithRanking()).ToList();

                        if (dsWithRanking.Count > 0)
                        {
                            int rank = dsWithRanking.ElementAt(0).Ranking = 1;
                            for (int i = 1; i < dsWithRanking.Count; i++)
                            {
                                if (dsWithRanking.ElementAt(i).Percent != dsWithRanking.ElementAt(i - 1).Percent)
                                    rank = i + 1;

                                dsWithRanking.ElementAt(i).Ranking = rank;
                            }
                        }

                        gv.DataSource = dsWithRanking;
                    }
                    else
                        gv.DataSource = dataSource;

                    gv.DataBind();

                    Label lbl = CreateCummulativeStatsLabel(lessThan, increment);

                    System.Web.UI.HtmlControls.HtmlGenericControl td = new System.Web.UI.HtmlControls.HtmlGenericControl("td");
                    td.Attributes.Add("style", "padding-right:25px;");
                    td.Attributes.Add("valign", "top");
                    td.Controls.Add(lbl);
                    td.Controls.Add(gv);

                    tr.Controls.Add(td);
                }

                lessThan -= increment;
            } while (lessThan > 0);

            if (numberOfGridsAdded > 1)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl tdWithPic = new System.Web.UI.HtmlControls.HtmlGenericControl("td");
                tdWithPic.Attributes.Add("valign", "top");

                System.Web.UI.HtmlControls.HtmlGenericControl img = new System.Web.UI.HtmlControls.HtmlGenericControl("img");
                img.Attributes.Add("src", "images/babyTable.png");
                img.Attributes.Add("alt", "");
                tdWithPic.Controls.Add(img);

                tr.Controls.Add(tdWithPic);                
            }

            table.Controls.Add(tr);
            pnlCummulativeStatsGrids.Controls.Add(table);

        }

        private Label CreateCummulativeStatsLabel(int lessThan, int increment)
        {
            /*<asp:Label ID="lblCummStatsUnder200" runat="server" Font-Bold="True" 
                    Font-Size="Large" ForeColor="Yellow" Text="150 picks or more */
            Label lbl = new Label();
            lbl.Font.Bold = true;
            lbl.Font.Size = FontUnit.Large;
            lbl.ForeColor = System.Drawing.Color.Yellow;
            lbl.Text = (lessThan == increment) ? "Less than " + increment + " picks" : "Between " + (lessThan - increment) + " and " + lessThan + " picks";
            lbl.Attributes.Add("style", "padding-bottom:5px;");
            return lbl;
        }

        private GridView CreateCummulativeStatsGrid()
        {
            /*
             <asp:GridView ID="gvCummulativeStatsUnder200" runat="server" BackColor="White" 
                    BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" 
                    EnableModelValidation="True" ForeColor="Black" GridLines="Vertical">
                    <AlternatingRowStyle BackColor="White" />
                    <FooterStyle BackColor="#CCCC99" />
                    <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
                    <RowStyle BackColor="#F7F7DE" />
                    <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                </asp:GridView>
             */
            GridView gv = new GridView();
            gv.BackColor = System.Drawing.Color.White;
            gv.BorderColor = GetFromHexCode("#DEDFDE");
            gv.BorderStyle = BorderStyle.None;
            gv.BorderWidth = new Unit(1);
            gv.CellPadding = 4;
            gv.EnableModelValidation = true;
            gv.ForeColor = System.Drawing.Color.Black;
            gv.GridLines = GridLines.Vertical;
            gv.AlternatingRowStyle.BackColor = System.Drawing.Color.White;
            gv.FooterStyle.BackColor = GetFromHexCode("#CCCC99");
            gv.HeaderStyle.BackColor = GetFromHexCode("#6B696B");
            gv.HeaderStyle.Font.Bold = true;
            gv.HeaderStyle.ForeColor = System.Drawing.Color.White;
            gv.PagerStyle.BackColor = GetFromHexCode("#F7F7DE");
            gv.PagerStyle.ForeColor = System.Drawing.Color.Black;
            gv.PagerStyle.HorizontalAlign = HorizontalAlign.Right;
            gv.RowStyle.BackColor = GetFromHexCode("#F7F7DE");
            gv.SelectedRowStyle.BackColor = GetFromHexCode("#CE5D5A");
            gv.SelectedRowStyle.Font.Bold = true;
            gv.SelectedRowStyle.ForeColor = System.Drawing.Color.White;

            return gv;

        }

        System.Drawing.Color GetFromHexCode(string hexCode)
        {
            return System.Drawing.ColorTranslator.FromHtml(hexCode);
        }
        
        private void loadMatchups(db.DBModelDataContext data, int week)
        {
            System.Data.DataTable matchups = CacheHelper.GetCachedGridViewMatchupsDataSource(week);
            gvMatchups.DataSource = matchups;
            gvMatchups.DataBind();
        }

        private void loadWeeklyStats(db.DBModelDataContext data, int week)
        {
            System.Data.DataTable gridTable = CacheHelper.GetCachedGridViewWeeklyStatsDataSource(week);

            if (gridTable.Rows.Count > 0)
            {
                gvStats.DataSource = gridTable;
                gvStats.DataBind();

                gvStats.Rows[gvStats.Rows.Count - 2].BackColor = GetFromHexCode("#6B696B");
                for (int i = 0; i < gvStats.Rows[0].Cells.Count; i++)
                {
                    if (i == 0)
                        gvStats.Rows[gvStats.Rows.Count - 2].Cells[i].ColumnSpan = gvStats.Rows[gvStats.Rows.Count - 2].Cells.Count;
                    else
                        gvStats.Rows[gvStats.Rows.Count - 2].Cells[i].Visible = false;
                }
            }
        }

        private List<CummulativeStatsGridObject> ConvertToCummulativeGridObjects(System.Data.DataTable allStats)
        {
            List<CummulativeStatsGridObject> result = new List<CummulativeStatsGridObject>();

            foreach (System.Data.DataRow dr in allStats.Rows)
            {
                CummulativeStatsGridObject obj = new CummulativeStatsGridObject();
                obj.Person = dr["Name"].ToString();
                obj.Percent = Convert.ToDouble(dr["Percent"]);
                obj.Ratio = dr["Ratio"].ToString();
                result.Add(obj);
            }
            return result;
        }
        
        class WeeklyStatsGridObject
        {
            public string Person { get; set; }
            public string PickDate { get; set; }
            public decimal Percentage { get; set; }
            public string Ratio { get; set; }
            public string Picks { get; set; }
        }

        class CummulativeStatsGridObject
        {
            public string Person { get; set; }
            public double Percent { get; set; }
            public string Ratio { get; set; }

            public CummulativeStatsGridObjectWithRanking ToGridWithRanking()            
            {
                CummulativeStatsGridObjectWithRanking result = new CummulativeStatsGridObjectWithRanking();
                result.Percent = this.Percent;
                result.Person = this.Person;
                result.Ratio = this.Ratio;
                result.Ranking = 0;
                return result;
            }
        }
        class CummulativeStatsGridObjectWithRanking : CummulativeStatsGridObject
        {
            public int Ranking { get; set; }
        }

        

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (Authentication.ValidateUser(txtUsername.Text, txtPassword.Text, false))
            {
                if (Request.Cookies[Authentication.AUTHENTICATED_USER_STRING] == null || Request.Cookies[Authentication.AUTHENTICATED_USER_ENC_PASSWORD_STRING] == null)
                {
                    DateTime stayLoggedOn = Convert.ToDateTime(ddlStayLoggedOn.SelectedValue);

                    HttpCookie userCookie = new HttpCookie(Authentication.AUTHENTICATED_USER_STRING);
                    userCookie.Value = Authentication.GetAuthenticatedUserName();
                    userCookie.Expires = stayLoggedOn;

                    Encryption enc = new Encryption();
                    HttpCookie userEncPWCookie = new HttpCookie(Authentication.AUTHENTICATED_USER_ENC_PASSWORD_STRING);
                    userEncPWCookie.Value = enc.Encrypt(txtPassword.Text);
                    userEncPWCookie.Expires = stayLoggedOn;

                    Response.Cookies.Add(userCookie);
                    Response.Cookies.Add(userEncPWCookie);

                    Common.SetStayLoggedOnValue(stayLoggedOn);
                }
                Response.Redirect(Request.RawUrl);
            }
            else
                lblerror.Text = "Either username does not exist, or password is incorrect!";            
        }

        protected void btnLogOut_Click(object sender, EventArgs e)
        {
            Authentication.LogUserOut();

            if (Request.Cookies[Authentication.AUTHENTICATED_USER_STRING] != null && Request.Cookies[Authentication.AUTHENTICATED_USER_ENC_PASSWORD_STRING] != null)
            {
                Response.Cookies[Authentication.AUTHENTICATED_USER_STRING].Expires = DateTime.Now;
                Response.Cookies[Authentication.AUTHENTICATED_USER_ENC_PASSWORD_STRING].Expires = DateTime.Now;
            }

            Response.Redirect("nflpicks.aspx");
        }

        protected void btnSubmitPicks_Click(object sender, EventArgs e)
        {
            List<string> picks = new List<string>();
            int picksToMake = 0;

            SetPicksAndPicksToMakeValuesByPickGrid(out picks, out picksToMake);            

            int selectedWeek = Convert.ToInt32(ddlWeeks.SelectedValue);

            if (picksToMake != picks.Count) lblPicksError.Text = "You must pick all games!!";
            else if (picksToMake != Common.DBModel().NFL_weeks.First(x => x.week == selectedWeek).games) lblPicksError.Text = "All matchups haven't been entered for week " + selectedWeek + " yet.";
            else
            {
                lblPicksError.Text = string.Empty;
                string result = string.Join(",", picks.ToArray());

                var data = Common.DBModel();
                db.NFL_userPick pick = new db.NFL_userPick();

                pick.picks = result;
                pick.week = selectedWeek;
                pick.username = Authentication.GetAuthenticatedUserName();
                pick.pick_dt = DateTime.Now;

                data.NFL_userPicks.InsertOnSubmit(pick);

                if (!string.IsNullOrEmpty(pick.username))
                {
                    data.SubmitChanges();

                    CacheHelper.GetCachedGridViewWeeklyStatsDataSource(week: selectedWeek, clearCache: true);
                    HttpContext.Current.Session["autoweek"] = selectedWeek.ToString();
                    Response.Redirect(Request.RawUrl);
                }
                else
                    lblerror.Text = "Your session expired. Log in and try again. The picks you were trying to make were: " + result;
            }
        }

        private void SetPicksAndPicksToMakeValuesByPickGrid(out List<string> picks, out int picksToMake)
        {
            picks = new List<string>();
            picksToMake = 0;

            foreach (System.Web.UI.WebControls.Table table in pnlPicksGrid.Controls.OfType<Table>())
            {
                foreach (System.Web.UI.WebControls.TableRow row in table.Controls.OfType<TableRow>())
                {
                    picksToMake++;
                    foreach (TableCell cell in row.Controls.OfType<TableCell>())
                    {
                        foreach (System.Web.UI.HtmlControls.HtmlInputRadioButton rb in cell.Controls.OfType<System.Web.UI.HtmlControls.HtmlInputRadioButton>())
                            if (rb.Checked) picks.Add(rb.Value);
                    }
                }
            }
        }

        protected void linkRefresh_Click(object sender, EventArgs e)
        {
            CacheHelper.GetCachedGridViewMatchupsDataSource(Convert.ToInt32(ddlWeeks.SelectedValue), clearCache: true);
            Response.Redirect("~/NFLPicks.aspx#ctl00_ContentPlaceHolder1_Label2");
        }

        protected void btnPostNew_Click(object sender, EventArgs e)
        {
            btnPostNew.Visible = false;
            txtComment.Visible = true;
            cbEmailOnReply.Visible = cbEmailOnReply.Checked = Common.CurrentUser.email != null;
            txtCommentTitle.Visible = true;
            lblStaticPromptForCommentTitle.Visible = true;
            btnSubmitComment.CommandArgument = "NEW";
            btnSubmitComment.Visible = true;
            lblReplyingTo.Text = string.Empty;
        }

        protected void btnSubmitComment_Command(object sender, CommandEventArgs e)
        {
            if (txtCommentTitle.Visible && (txtCommentTitle.Text.Length < 5 || txtCommentTitle.Text.Length > 40))
            {
                lblCommentPostingError.Text = lblStaticPromptForCommentTitle.Text;
                return;
            }

            if (txtComment.Text.Length == 0)
            {
                lblCommentPostingError.Text = "You didn't enter anything for the comment.";
                return;
            }

            int? refId = null;
            string title = null;
            if (e.CommandArgument.ToString() != "NEW")
                refId = Convert.ToInt32(e.CommandArgument);
            else title = txtCommentTitle.Text;

            int week = Convert.ToInt32(ddlWeeks.SelectedValue);

            Common.AddNewNFLForumComment(week, Authentication.GetAuthenticatedUserName(), refId, txtComment.Text, title, cbEmailOnReply.Checked);
            int topicId = Common.GetTopicId(refId);
            HttpContext.Current.Session["defaultTopicId"] = topicId;

            HttpContext.Current.Session["autoweek"] = week.ToString();
            Response.Redirect("~/NFLPicks.aspx#forumTopic-" + topicId);
            
        }

        protected void ddlWeeks_SelectedIndexChanged(object sender, EventArgs e)
        {
            // don't think i need any of this anymore because of hard redirect on form load now but whatever
            cbEmailOnReply.Visible = false;
            lblReplyingTo.Text = string.Empty;
            txtComment.Visible = false;
            txtCommentTitle.Visible = false;
            btnSubmitComment.Visible = false;
            //btnPostNew.Visible = true;

            //loadAllDataGrids();
            //buildForum();

        }

        #region Admin Controls

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            int selectedWeek = Convert.ToInt32(ddlWeeksAdmin.SelectedValue);
            var week = Common.DBModel().NFL_weeks.First(x => x.week == selectedWeek);
            int numGames = week.games.Value;
            string weekText = getWeekText(selectedWeek);

            if (numGames == Common.DBModel().NFL_Matchups.Count(x => x.week == selectedWeek))
            {
                lblAdminError.Text = "There are only " + numGames + " games in " + weekText + ".";
            }

            else if (Common.DBModel().NFL_Matchups.Count(x => x.home == ddlHome.Text && x.away == ddlVis.Text && x.week == selectedWeek) > 0)
            {
                lblAdminError.Text = "You already added this matchup for this week.";
            }

            else if (Common.DBModel().NFL_Matchups.Count(x => x.week == selectedWeek && (x.home == ddlHome.Text || x.away == ddlHome.Text)) > 0)
            {
                lblAdminError.Text = ddlHome.Text + " are already playing in " + weekText;
            }
            else if (Common.DBModel().NFL_Matchups.Count(x => x.week == selectedWeek && (x.home == ddlVis.Text || x.away == ddlVis.Text)) > 0)
            {
                lblAdminError.Text = ddlVis.Text + " are already playing in " + weekText;
            }
            else if (ddlHome.Text == ddlVis.Text)
            {
                lblAdminError.Text = "A team can't play itself, dummy.";
            }

            else
            {
                lblAdminError.Text = string.Empty;

                var data = Common.DBModel();
                db.NFL_Matchup matchup = new db.NFL_Matchup();

                matchup.home = ddlHome.Text;
                matchup.away = ddlVis.Text;
                matchup.week = selectedWeek;
                matchup.year_week = data.NFL_weeks.First(x => x.week == selectedWeek).year_week;

                data.NFL_Matchups.InsertOnSubmit(matchup);
                data.SubmitChanges();

                Common.AutoLoadAdminPanel = true;

                bool publishWeek;
                lblAdminStatus.Text = displayCurrentMatchupsForAdminPanel(selectedWeek, out publishWeek);
                if (publishWeek)
                    Common.PublishNflWeek(selectedWeek);
            }
        }

        private string getWeekText(int selectedWeek)
        {
            var week = Common.DBModel().NFL_weeks.First(x => x.week == selectedWeek);
            return logic.Helpers.DisplayCodeString(((WeekTypes)week.weekTypeId).ToString()) + " week " + week.year_week;
        }

        private string displayCurrentMatchupsForAdminPanel(int selectedWeek)
        {
            bool publishWeek = false;
            return displayCurrentMatchupsForAdminPanel(selectedWeek, out publishWeek);
        }

        private string displayCurrentMatchupsForAdminPanel(int selectedWeek, out bool publishWeek)
        {
            string result = string.Empty;
            var data = Common.DBModel();
            var week = data.NFL_weeks.First(x => x.week == selectedWeek);
            if (week.games > week.NFL_Matchups.Count)
            {
                publishWeek = false;

                result = "Current matchups: <br>";
                int count = 0;
                foreach (var matchup in Common.DBModel().NFL_Matchups.Where(x => x.week == selectedWeek))
                {
                    count++;
                    result += matchup.away + " @ " + matchup.home + "<br>";
                }
                if (count == 0)
                {
                    result += "NO MATCHUPS ENTERED YET";
                }
            }
            else
            {
                publishWeek = true;
                result = "All matchups have been added for " + getWeekText(selectedWeek);
            }
            return result;
        }

        protected void btnAddTeam_Click(object sender, EventArgs e)
        {
            throw new Exception("btnAddTeam is no longer implemented");

            //if (Common.DBModel().NFL_teams.Count() == 32)
            //{
            //    lblAdminError.Text = "There are only 32 teams in the NFL and you already added that many.";
            //}
            //else
            //{
            //    lblAdminError.Text = string.Empty;

            //    var data = Common.DBModel();
            //    db.NFL_team newTeam = new db.NFL_team();

            //    newTeam.location = txt_location.Text;
            //    newTeam.name = txtTeamName.Text;
            //    newTeam.division = ddlDivisions.SelectedValue;

            //    data.NFL_teams.InsertOnSubmit(newTeam);
            //    data.SubmitChanges();

            //    txt_location.Text = string.Empty;
            //    txtTeamName.Text = string.Empty;
            //    ddlVis.DataSource = ddlHome.DataSource = Common.DBModel().NFL_teams.OrderBy(x => x.name).Select(y => y.name);
            //    ddlVis.DataBind();
            //    ddlHome.DataBind();

            //    buildTeamsGrid();

            //}
        }

        #endregion

        protected void lnkShowLeagueStandings_Click(object sender, EventArgs e)
        {
            if (lnkShowLeagueStandings.Text.ToLower().StartsWith("show"))
            {
                pnlLeagueStandings.Visible = true;
                lnkShowLeagueStandings.Text = "Hide League Standings";
            }
            else
            {
                pnlLeagueStandings.Visible = false;
                lnkShowLeagueStandings.Text = "Show League Standings";
            }

            lnkShowLeagueStandings2.Text = lnkShowLeagueStandings.Text;
            lnkShowLeagueStandings3.Text = lnkShowLeagueStandings.Text;
            
        }

        protected void lnkShowDivisionStandings_Click(object sender, EventArgs e)
        {
            if (lnkShowDivisionStandings.Text.ToLower().StartsWith("show"))
            {
                pnlDivisionStandings.Visible = true;
                lnkShowDivisionStandings.Text = "Hide Division Standings";
            }
            else
            {
                pnlDivisionStandings.Visible = false;
                lnkShowDivisionStandings.Text = "Show Division Standings";
            }

            lnkShowDivisionStandings2.Text = lnkShowDivisionStandings.Text;
            lnkShowDivisionStandings3.Text = lnkShowDivisionStandings.Text;
        }

        protected void ddlCummStatGrouping_SelectedIndexChanged(object sender, EventArgs e)
        {
            Common.SetCummulativeStatsGrouping(Convert.ToInt32(ddlCummStatGrouping.SelectedValue));
        }

        private int GetCurrentYearId()
        {
            if (Session["CurrentYearId"] == null)
            {
                int sportId = Common.GetCurrentSportId();
                int yearId = Common.DBModel().NFL_years.Where(y => y.sport_id == sportId).Max(y => y.id);
                SetCurrentYearId(yearId);
            }

            return Convert.ToInt32(Session["CurrentYearId"]);
        }
        private void SetCurrentYearId(int id)
        {
            Session["CurrentYearId"] = id;
            Session["CurrentYearDisplay"] = Common.DBModel().NFL_years.First(x => x.id == id).text;
        }
        
        protected void ddlYears_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetCurrentYearId(Convert.ToInt32(ddlYears.SelectedValue));
            Response.Redirect(Request.RawUrl);
        }

        protected void ddlSports_SelectedIndexChanged(object sender, EventArgs e)
        {
            Common.SetCurrentSportId(Convert.ToInt32(ddlSports.SelectedValue));
            Session["CurrentYearId"] = null;
            Session["CurrentYearDisplay"] = null;
            Response.Redirect(Request.RawUrl);
        }

        protected void btnPostBack_Click(object sender, EventArgs e)
        {

        }

        protected void ddlWeeksAdmin_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblAdminStatus.Text = displayCurrentMatchupsForAdminPanel(Convert.ToInt32(ddlWeeksAdmin.SelectedValue));
            Common.AutoLoadAdminPanel = true;
        }

        protected void btnDoneAddingMatchups_Click(object sender, EventArgs e)
        {
            Common.AutoLoadAdminPanel = false;
        }

        protected void btnDeleteAllMatchups_Click(object sender, EventArgs e)
        {
            Common.AutoLoadAdminPanel = true;
            int selectedWeek = Convert.ToInt32(ddlWeeksAdmin.SelectedValue);

            if (Common.DBModel().NFL_weeks.First(x => x.week == selectedWeek).NFL_userPicks.Count == 0)
            {
                Common.DeleteAllMatchups(selectedWeek);
                lblAdminStatus.Text = displayCurrentMatchupsForAdminPanel(selectedWeek);
            }
            else 
            {
                lblAdminError.Text = "You cannot enter these matchups again because picks have already been made. Contact IT.";
            }
        }

        protected void btnSavePicksForLater_Click(object sender, EventArgs e)
        {
            List<string> picks;
            int i;
            SetPicksAndPicksToMakeValuesByPickGrid(out picks, out i);

            Common.SetUserSetting("PICKS-WEEK-" + Convert.ToInt32(ddlWeeks.SelectedValue), string.Join(",", picks.ToArray()));

            lblSavePicksForLaterStatus.Text = "Done.";
        }

        protected void btnSaveEmailSubscriptions_Click(object sender, EventArgs e)
        {
            foreach (ListItem item in cblEmailSubscriptions.Items)
                if (item.Selected)
                    HomeAppsLib.EmailSubscriptions.AddSubscription(Convert.ToInt32(item.Value), Authentication.GetAuthenticatedUserName());
                else
                    HomeAppsLib.EmailSubscriptions.RemoveSubscription(Convert.ToInt32(item.Value), Authentication.GetAuthenticatedUserName());
        }

        protected void btnSaveEmail_Click(object sender, EventArgs e)
        {
            if (!LibCommon.IsValidEmail(txtEmailAddress.Text))
            {
                lblEmailSignupValidationMessage.Text = "You must enter valid email.";
                return;
            }
            else
                lblEmailSignupValidationMessage.Text = string.Empty;

            string username = Authentication.GetAuthenticatedUserName();
            var data = HomeAppsLib.LibCommon.DBModel();
            var user = data.users.First(x => x.name == username);
            user.email = txtEmailAddress.Text;           
            data.SubmitChanges();

            foreach (var subType in data.emailSubscriptionTypes.Where(x => x.autoEnroll))
                HomeAppsLib.EmailSubscriptions.AddSubscription(subType.Id, user.name);

            initializeOtherControls();
        }

        protected void btnPostBackForWeeklyStats_Click(object sender, EventArgs e)
        {

        }

        protected void lnkViewWinLossStats_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/WinLossStats.aspx?yearId=" + GetCurrentYearId());
        }

        protected void btnAddThisUser_Click(object sender, EventArgs e)
        {
            List<string> usersToAdd = new List<string>();
            if (Common.getUsersToAddToMatchupsGrid() != null)
                usersToAdd = Common.getUsersToAddToMatchupsGrid().ToList();
            usersToAdd.Add(ddlUsersThatCanByAdded.Text);
            Common.setUsersToAddToMatchupsGrid(usersToAdd.ToArray());

            Response.Redirect("~/NFLPicks.aspx");
        }

        protected void btnAddTheBetUsers_Click(object sender, EventArgs e)
        {
            Common.setUsersToAddToMatchupsGrid(Authentication.BetUsers(GetCurrentYearId()).ToArray());
            Response.Redirect("~/NFLPicks.aspx");
        }

        protected void btnResetAddUsers_Click(object sender, EventArgs e)
        {
            Common.setUsersToAddToMatchupsGrid(null);
            Response.Redirect("~/NFLPicks.aspx");
        }
    }
}