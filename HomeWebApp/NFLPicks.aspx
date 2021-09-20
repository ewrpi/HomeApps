<%@ Page Title="" Language="C#" MasterPageFile="~/site.Master" AutoEventWireup="true" CodeBehind="NFLPicks.aspx.cs" Inherits="HomeWebApp.NFLPicks" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">

        $("#ctl00_ContentPlaceHolder1_btnPostNew").click(function () {
            setGlobalWait();
        });

    function showHide(id) {

        var el = customGetElementById(id);
        if (el.style.display == 'none') {
            el.style.display = 'block';
        }
        else {
            el.style.display = 'none';
        }
    }

    function showAll() {

        var els = document.getElementsByName("discussionPosts");
        if (els.length == 0) els = getElementsByNameInFuckingIE("div", "discussionPosts");        
        for (var i = 0; i < els.length; i++) {
            els[i].style.display = 'block';
        }

    }    

    function hideAll() {
        var els = document.getElementsByName("discussionPosts");
        if (els.length == 0) els = getElementsByNameInFuckingIE("div", "discussionPosts");        
        for (var i = 0; i < els.length; i++) {
            els[i].style.display = 'none';
        }
    }

    function getElementsByNameInFuckingIE(tagName, name) {
        //alert("stop fucking using internet explorer!!");
        var els = [];
        var divs = document.getElementsByTagName(tagName)
        for (var i = 0; i < divs.length; i++) {
            if (divs[i].getAttribute("name") == name) els.push(divs[i]);
        }
        return els;
    }

    function showUser() {

        hideAll();

        var name = customGetElementById("userDD").value;
        var els = getPostsFor(name);        
        for (var i = 0; i < els.length; i++) {
            els[i].style.display = 'block';
        }
    }

    function getPostsFor(name) {
        var divs = document.getElementsByTagName("div");
        var els = [];

        for(var i =0; i<divs.length; i++){
            if (divs[i].getAttribute("person") == name) {
                els.push(divs[i]);               
            }
        }
        return els;

    }
    function showHide(id) {
        var el = customGetElementById(id);
        if (el.style.display == 'none') {
            el.style.display = 'block';
        }
        else {
            el.style.display = 'none';
        }
    }

    addCBsToWeeklyStats();
    function addCBsToWeeklyStats() {
        if (document.readyState != "complete") {
            setTimeout('addCBsToWeeklyStats()', 10);
            return;
        }

        var table = customGetElementById("ContentPlaceHolder1_gvStats");

        var cell1 = table.rows[0].insertCell(0);
        cell1.innerHTML = "Hide";
        for (var i = 1; i < table.rows.length - 2; i++) {
            var cell = table.rows[i].insertCell(0);
            cell.innerHTML = "<input type='checkbox' name='hidePersonCB' onclick='hidePerson()' />";
        }
        for (var i = table.rows.length - 2; i < table.rows.length; i++) {
            var cell = table.rows[i].insertCell(0);
        }
    }

    function hidePerson() {
        var table = customGetElementById("ContentPlaceHolder1_gvStats");
        var hideCBs = document.getElementsByName("hidePersonCB");
        for (var i = 0; i < hideCBs.length; i++) {
            if (hideCBs[i].checked) {
                table.deleteRow(i + 1);
            }
        }
    }
    function openWinLossStats() {
        var e = customGetElementById("ContentPlaceHolder1_ddlYears");
        var yearId = e.options[e.selectedIndex].value;
        window.open("/WinLossStats.aspx?yearId=" + yearId);
    }

    function customGetElementById(id){
        if (document.getElementById(id))
            return document.getElementById(id);
        else
            return document.getElementById('ctl00_' + id);
    }

</script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div align="center">
    <br />
    
    <asp:Panel ID="pnlLogInOrRegister" runat="server" Visible="False">
    <font size="10" color="yellow">
    Log in! Or click <a href="Register.aspx">here</a> to register!
    </font>

    <table>
    <tr align="left">
        <td class="fieldLabel">Name: </td>
        <td>
            <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox></td>
        
    </tr>
    <tr align="left">
        <td class="fieldLabel">Password: </td>
        <td>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox></td>
    </tr>
    <tr align="left">
        <td></td>
        <td>
            <asp:Button ID="btnSubmit" runat="server" Text="Log In" 
                onclick="btnSubmit_Click" Font-Size="X-Large" OnClientClick="setGlobalWait()" /></td>
        <td><a href="/ForgotPassword.aspx" style="color:Yellow;">I already have an account but can't login</a></td>
    </tr>
     

    </table><br />
        <asp:Label ID="lblerror" runat="server" Font-Bold="True" Font-Size="X-Large" 
            ForeColor="Yellow"></asp:Label>

    </asp:Panel>     

    <asp:Panel ID="pnlLoggedIn" runat="server" Visible="False">    
    
    <div align="left">
        <div style="float:right;">
            <div style="float:right;margin-bottom:25px;"> 
            </div>
            <br />
        </div>
        <div style="clear:both;"></div>

        <%if (HomeWebApp.Authentication.IsAdminUser())
          {
              string display = HomeWebApp.Common.AutoLoadAdminPanel ? "block" : "none";%>

          <span style="text-decoration:underline;color:Green;cursor:pointer;" onclick="showHide('adminPanel')">Show/Hide Admin Panel</span>
        <div id="adminPanel" align="center" style="display:<%= display %>;">
        <hr />
          <br />          
          <br />
        <font size="5" color="green">ADMIN Only</font>  <br />
        <font size="2" color="yellow">Add matchup for week </font> 
        <asp:DropDownList ID="ddlWeeksAdmin" runat="server" AutoPostBack="True" 
                onselectedindexchanged="ddlWeeksAdmin_SelectedIndexChanged">
        </asp:DropDownList>
        <br />
        <asp:Label ID="lblAdminError" runat="server" Font-Size="XX-Large" ForeColor="Yellow" Font-Bold="True"></asp:Label>
        <br />
          Visitor <asp:DropDownList ID="ddlVis" runat="server"></asp:DropDownList> @ <asp:DropDownList ID="ddlHome" runat="server"></asp:DropDownList> Home 
        
        <br /><br />
        <asp:Button ID="btnAdd" runat="server" Text="Add Matchup" 
            onclick="btnAdd_Click" OnClientClick="setGlobalWait()" />
            <br />
            <br />
            <asp:Button ID="btnDoneAddingMatchups" runat="server" 
                onclick="btnDoneAddingMatchups_Click" Text="Done" OnClientClick="setGlobalWait()" />
        <br />
        <br />
        <asp:Label ID="lblAdminStatus" runat="server" Font-Bold="True" 
            Font-Size="Medium" ForeColor="Yellow"></asp:Label>
            <br />
            <br />
            <asp:Button ID="btnDeleteAllMatchups" runat="server" 
                onclick="btnDeleteAllMatchups_Click" Text="Start Over" OnClientClick="setGlobalWait()" />
            
        
        <br />

          <hr />
        </div>
          <%} %>


        <div>
            <font color='yellow' size="5">Hi, <%=HomeWebApp.Authentication.GetAuthenticatedUserName() %>!
            
            <div style="float:right;">
                <asp:Button 
                ID="btnLogOut" runat="server" onclick="btnLogOut_Click" Text="Log Out" OnClientClick="setGlobalWait()" />
            </div>
            <br />
            Welcome to <%=Session["CurrentSportDisplay"]%> Season <%=Session["CurrentYearDisplay"] %>.
            <div style="float:right;">
                <font color="yellow" size="2">
                Change sport <asp:DropDownList ID="ddlSports" runat="server" 
                    AutoPostBack="True" onselectedindexchanged="ddlSports_SelectedIndexChanged" >
                    </asp:DropDownList>
                    <br />
                    Change year <asp:DropDownList ID="ddlYears" runat="server" AutoPostBack="True" 
                    onselectedindexchanged="ddlYears_SelectedIndexChanged">
                    </asp:DropDownList>
                </font>
            </div>
            
            
            
            <br />
            Choose week:  
                <asp:DropDownList ID="ddlWeeks" runat="server" AutoPostBack="True"             
                    onselectedindexchanged="ddlWeeks_SelectedIndexChanged">
                </asp:DropDownList>
            </font>
        </div>
    </div>

    <br />
    <hr />
    <br />

        <asp:Label ID="lblPromptToChooseWeek" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow"></asp:Label>

    <asp:Panel ID="pnlMakePicks" runat="server" Visible="False">
    
        <asp:Panel ID="pnlMakePicksControls" runat="server">

    <asp:Label ID="lblPromptToMakePicks" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow" Text="Make your picks!"></asp:Label>

            <span style="float:right;">
                <a href="/NFLMakePicksMobile.aspx">Mobile</a>
            </span>
            <span style="clear:both;"></span>

            <br />
            <asp:Label ID="lblExpiration" runat="server" Font-Bold="True" Font-Size="Large" 
                ForeColor="Yellow"></asp:Label>
            <br /> <br />
            
            <span style="text-decoration:underline;color:Yellow; font-size:x-large;cursor:pointer;" onclick="openWinLossStats()">Win Loss Stats</span><br />
        <asp:LinkButton ID="lnkShowLeagueStandings2" runat="server" Font-Size="X-Large" 
                ForeColor="#FFFF66" onclick="lnkShowLeagueStandings_Click">Show League Standings</asp:LinkButton><br />
        <asp:LinkButton ID="lnkShowDivisionStandings2" runat="server" Font-Size="X-Large" 
                ForeColor="Yellow" onclick="lnkShowDivisionStandings_Click">Show Division Standings</asp:LinkButton><br />
        <%if (Request.QueryString["quickpicks"] == "true"){ %>
        <br /><div style="color:Yellow;font-size:X-Large;" class="jsLink" onclick="document.location.reload();">Randomize</div><br />
        <asp:Button ID="btnSubmitPicks2" runat="server" Text="Submit My Picks!" 
            onclick="btnSubmitPicks_Click" Height="50px" Width="230px" OnClientClick="setGlobalWait()" />
            <br /><br />
        <%} %>
        
                   
        <table><tr>
        <td>
            <img src="images/visitors.jpg" style="width:250px;" />
        </td>
        <td>
            <asp:Panel ID="pnlPicksGrid" runat="server">
            </asp:Panel>
        </td>
        <td>
            <img src="images/home.jpg" style="width:250px;" />
        </td>
        </tr></table>

        <%if (Request.QueryString["quickpicks"] == "true"){ %>
        <br /><div style="color:Yellow;font-size:X-Large;" class="jsLink" onclick="document.location.reload();">Randomize</div><br />
        <%} %>

        <asp:Button ID="btnSavePicksForLater" runat="server" Text="Save For Later" 
                Height="27px" Width="99px" onclick="btnSavePicksForLater_Click" OnClientClick="setGlobalWait()" />
        <asp:Label ID="lblSavePicksForLaterStatus" runat="server" Font-Bold="True" Font-Size="Large" 
                ForeColor="Yellow"></asp:Label>
        <br />

        <asp:LinkButton ID="lnkShowLeagueStandings" runat="server" Font-Size="X-Large" 
                ForeColor="#FFFF66" onclick="lnkShowLeagueStandings_Click">Show League Standings</asp:LinkButton><br />
        <asp:LinkButton ID="lnkShowDivisionStandings" runat="server" Font-Size="X-Large" 
                ForeColor="Yellow" onclick="lnkShowDivisionStandings_Click">Show Division Standings</asp:LinkButton><br />

        <br />
        <asp:Button ID="btnSubmitPicks" runat="server" Text="Submit My Picks!" 
            onclick="btnSubmitPicks_Click" Height="50px" Width="230px" OnClientClick="setGlobalWait()" />
            <br />
            &nbsp;
        
        

        </asp:Panel>
        <asp:Label ID="lblPicksError" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow"></asp:Label><br />            
        
            <asp:Label ID="lblInitialPrompt" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow"></asp:Label>       

    </asp:Panel>
    
        <asp:Panel ID="pnlCrazyArrows" runat="server">
        </asp:Panel>

        <asp:Panel ID="pnlStats" runat="server">     
           
<div align="left">

<span id="showHideEmailSubscriptions" class="jsLink" style="color:Yellow;float:right;" onclick="showHide('emailSubscriptions')">Show/Hide Email Subscriptions</span>
    <div id="emailSubscriptions" style="display:<%= (HomeWebApp.Common.UserHasEmailSub ? "none" : "block") %>;">
        <asp:Label ID="Label5" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow" Text="Email Subscriptions"></asp:Label><br />
        <% if (HomeWebApp.Common.UserHasEmail)
           { %>
        <asp:CheckBoxList ID="cblEmailSubscriptions" runat="server" ForeColor="Yellow">
        </asp:CheckBoxList>
        <asp:Button ID="btnSaveEmailSubscriptions" runat="server" 
            Text="Save Subcriptions" onclick="btnSaveEmailSubscriptions_Click" OnClientClick="setGlobalWait()" />

        <%}
           else
           { %>
           <span style="color:Yellow;">Email: </span> 
        <asp:TextBox ID="txtEmailAddress" runat="server"></asp:TextBox> 
        &nbsp;<asp:Button ID="btnSaveEmail" runat="server" Text="Sign Up" 
            onclick="btnSaveEmail_Click" OnClientClick="setGlobalWait()" />
            &nbsp;<asp:Label ID="lblEmailSignupValidationMessage" runat="server" 
            Font-Bold="True" Font-Size="XX-Large" ForeColor="Yellow"></asp:Label>
        <%} %>        
        <hr />        
    </div>
    <br />

        <asp:Label ID="Label1" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow" Text="Weekly Stats"></asp:Label>&nbsp;<asp:Button 
        ID="btnPostBackForWeeklyStats" runat="server" 
        onclick="btnPostBackForWeeklyStats_Click" Text="Reload" OnClientClick="setGlobalWait()" />
    <br />
    <div style="width:1000px;overflow:visible;padding-top:15px;">
        <asp:GridView ID="gvStats" runat="server" BackColor="White" 
            BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" 
            EnableModelValidation="True" ForeColor="Black" GridLines="Vertical">
            <AlternatingRowStyle BackColor="White" />
            <FooterStyle BackColor="#CCCC99" />
            <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
            <RowStyle BackColor="#F7F7DE" />
            <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
        </asp:GridView>
        <br />
    </div>
    <hr />
    <asp:Label ID="Label3" runat="server" Font-Bold="True" Font-Size="XX-Large" 
        ForeColor="Yellow" Text="Cummulative Stats"></asp:Label>
    <br />
    <font color="yellow">Break up in groups of </font>
    <asp:DropDownList ID="ddlCummStatGrouping" runat="server" AutoPostBack="True" 
        onselectedindexchanged="ddlCummStatGrouping_SelectedIndexChanged">
    </asp:DropDownList>
    &nbsp;    
            <asp:CheckBox ID="cbCummStatsPre" runat="server" 
            Text="Include Pre Season" ForeColor="Yellow" Checked="True" />
        &nbsp;
            <asp:CheckBox ID="cbCummStatsRegular" runat="server" 
            Text="Include Regular Season" ForeColor="Yellow" Checked="True" />
        &nbsp;
            <asp:CheckBox ID="cbCummStatsPost" runat="server" 
            Text="Include Post Season" ForeColor="Yellow" Checked="True" />
        &nbsp;
    <asp:DropDownList ID="ddlCummStatsRemoveYearFilter" runat="server">
        <asp:ListItem Value="false">This Year</asp:ListItem>
        <asp:ListItem Value="true">All Years</asp:ListItem>
    </asp:DropDownList>
        &nbsp;
    <asp:Button ID="btnPostBack" runat="server" Text="Refresh" 
            onclick="btnPostBack_Click" OnClientClick="setGlobalWait()" />

                    <br /><br />

    <asp:Panel ID="pnlCummulativeStatsGrids" runat="server">
    </asp:Panel>
    
<%--    <% if (HomeWebApp.Authentication.IsABetUser())
       { %>--%>
    <asp:Panel ID="pnlTheBet" runat="server">
    <br />
    <hr />
    <asp:Label ID="lblTheBet" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow" Text="The Bet (excludes pre-season and pro bowl)"></asp:Label>
    <asp:GridView ID="GridView1" runat="server">
    </asp:GridView>

    <asp:GridView ID="gvTheBet" runat="server" BackColor="White" 
                            BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" 
                            EnableModelValidation="True" ForeColor="Black" GridLines="Vertical">
                            <AlternatingRowStyle BackColor="White" />
                            <FooterStyle BackColor="#CCCC99" />
                            <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
                            <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
                            <RowStyle BackColor="#F7F7DE" />
                            <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
                        </asp:GridView>
    </asp:Panel>
    
<%--<%} %>--%>
    <br />
    <hr />
    
           
                    <asp:Label ID="Label2" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow" Text="Matchups this Week"></asp:Label>
    &nbsp;&nbsp;&nbsp;
                    <asp:LinkButton ID="linkRefresh" ForeColor="Yellow" runat="server" onclick="linkRefresh_Click">Refresh</asp:LinkButton>        
    &nbsp;&nbsp;&nbsp;
                    <%--<a target="_blank" href="http://www.nfl.com/liveupdate/scorestrip/ss.xml" style="color:yellow;">Check API</a>--%>
                    <%--<a target="_blank" href="http://static.nfl.com/liveupdate/scores/scores.json" style="color:yellow;">Check API</a>--%>
    <a target="_blank" href="http://site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard" style="color:yellow;">Check API</a>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
             
    <asp:DropDownList ID="ddlUsersThatCanByAdded" runat="server" Visible="False">
    </asp:DropDownList> 
    <asp:Button ID="btnAddThisUser" runat="server" Text="Add This User" 
        onclick="btnAddThisUser_Click" Visible="False" />

            &nbsp;<asp:Button ID="btnAddTheBetUsers" runat="server" 
         Text="Add The Bet Users" onclick="btnAddTheBetUsers_Click" 
        Visible="False" />
    &nbsp;<asp:Button ID="btnResetAddUsers" runat="server" 
         Text="Reset" onclick="btnResetAddUsers_Click" Visible="False" />
    <br />
    <asp:GridView ID="gvMatchups" runat="server" BackColor="White" 
        BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" 
        EnableModelValidation="True" ForeColor="Black" GridLines="Vertical">
        <AlternatingRowStyle BackColor="White" />
        <FooterStyle BackColor="#CCCC99" />
        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
        <RowStyle BackColor="#F7F7DE" />
        <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
    </asp:GridView>
    <br />
    <span onclick="openWinLossStats()" 
        style="text-decoration:underline;color:Yellow; font-size:x-large;cursor:pointer;">
    Win Loss Stats</span><br />
            <asp:LinkButton ID="lnkShowLeagueStandings3" runat="server" Font-Size="X-Large" 
                ForeColor="#FFFF66" onclick="lnkShowLeagueStandings_Click">Show League Standings</asp:LinkButton><br />
        <asp:LinkButton ID="lnkShowDivisionStandings3" runat="server" Font-Size="X-Large" 
                ForeColor="Yellow" onclick="lnkShowDivisionStandings_Click">Show Division Standings</asp:LinkButton><br />
</div>

            
        </asp:Panel>

         <asp:Panel ID="pnlLeagueStandings" runat="server" Visible="False">
        </asp:Panel>
        <br />
        <asp:Panel ID="pnlDivisionStandings" runat="server" Visible="False">
        </asp:Panel>

        <br />
        <hr />
        <asp:Panel ID="pnlForumControls" runat="server" Visible="False">
        <div align="left">
         <asp:Label ID="Label4" runat="server" Font-Bold="True" 
            Font-Size="XX-Large" ForeColor="Yellow" Text="Discussion Board"></asp:Label>
                <br /><br />
    <asp:Label ID="lblReplyingTo" runat="server" Font-Bold="True" 
                    Font-Size="XX-Large" ForeColor="Yellow" Visible="True"></asp:Label>
    
        <asp:Button ID="btnPostNew" runat="server" Text="Post New Topic" 
            Visible="True" onclick="btnPostNew_Click" Height="54px" Width="214px" OnClientClick="setGlobalWait()" /><br />
    
            
        <table>
            <tr>
                <td valign="bottom">
        <asp:TextBox ID="txtCommentTitle" runat="server" Visible="False" Width="193px" 
                        BorderColor="Black" BorderStyle="Solid"></asp:TextBox>&nbsp;<asp:Label 
                    ID="lblStaticPromptForCommentTitle" runat="server" 
                    Text="** Title must be between 5 and 40 characters" Visible="False"></asp:Label>
                <br />
                <asp:TextBox ID="txtComment" runat="server" Height="85px" TextMode="MultiLine" 
                    Visible="False" Width="487px" BorderColor="Black" BorderStyle="Solid"></asp:TextBox>
        
                </td>

                <td valign="bottom">
                    <asp:CheckBox ID="cbEmailOnReply" runat="server" ForeColor="Yellow" 
                        Text="Email Me Replies" Visible="False" /><br />
                    <asp:Button ID="btnSubmitComment" runat="server" Text="Submit" Visible="False" oncommand="btnSubmitComment_Command" Height="44px" Width="115px" OnClientClick="setGlobalWait()" />
                </td>
            </tr>
        </table>
    
                <br />
                <asp:Label ID="lblCommentPostingError" runat="server" Font-Bold="True" 
                    Font-Size="XX-Large" ForeColor="Yellow" Visible="True"></asp:Label>
            
        <asp:Panel ID="pnlForum" runat="server">
                </asp:Panel>
        </div>
        </asp:Panel>

         

        <br />
        

    </asp:Panel>
</div>
</asp:Content>