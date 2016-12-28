<%@ Page Title="" Language="C#" MasterPageFile="~/site.Master" AutoEventWireup="true" CodeBehind="Eric.aspx.cs" Inherits="HomeWebApp.Eric" MaintainScrollPositionOnPostBack="true"%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


<div align="left">
    <asp:TextBox ID="txtPW" runat="server" TextMode="Password"></asp:TextBox><br />
    <asp:Button ID="btnDisplay" runat="server" 
        onclick="btnDisplay_Click" />
    <asp:Label ID="lblError" runat="server"></asp:Label>
    <br />
    <asp:CheckBox ID="cbTestMode" runat="server" AutoPostBack="True" 
        oncheckedchanged="cbTestMode_CheckedChanged" Text="Test Mode" /><br />
        <asp:CheckBox ID="cbRapidFire" runat="server" AutoPostBack="True" 
        oncheckedchanged="cbRapidFire_CheckedChanged" Text="Rapid Fire" />
    <br />
    <br />
    <asp:Label ID="lblMachineName" runat="server" Text="Label"></asp:Label>
    <br /><br />

    <asp:Panel ID="pnl_main" runat="server" Visible="False">

    <div style="border-style:solid;">
    Session: <br />
        <asp:Label ID="lblSession" runat="server" Text="Label"></asp:Label>
    </div>
    <div style="border-style:solid;">
    Users: <br />
        <asp:Label ID="lblAllUsers" runat="server" Text="Label"></asp:Label>
    </div>
    <div style="border-style:solid;">
    Headers: <br />
        <asp:Label ID="lblHeaders" runat="server" Text="Label"></asp:Label>
    </div>

    <table>
        <tr>
            <td align="right">IP Address: </td>
            <td align="left">
                <asp:TextBox ID="txtIPAddress" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td align="right"></td>
            <td align="left">
                <asp:Button ID="btnSendIPNotificationEmail" runat="server" Text="Send" 
                    onclick="btnSendIPNotificationEmail_Click" />
                <asp:CheckBox ID="cb_sendAddlInfo" runat="server" Text="Send Additional Info" />
                    
            </td>
        </tr>        
    </table>
        <br />
        <asp:Label ID="lblTotalVisitors" runat="server"></asp:Label>
        <br />

    <div style="height:500px;overflow:scroll">
        <asp:GridView ID="gvVisitors" runat="server" EnableModelValidation="True" 
            onrowcommand="gvVisitors_RowCommand">
            <Columns>
                <asp:ButtonField CommandName="select" Text="Select" />
                <asp:ButtonField CommandName="retry" Text="Retry" />
            </Columns>
        </asp:GridView>
    </div><br />
    
        <%if (HttpContext.Current.Session["ipdata"] == null) HttpContext.Current.Session["ipdata"] = string.Empty; %>
        <%Response.Write(HttpContext.Current.Session["ipdata"].ToString()); %>
        
    
        <br />
        <div style="height:250px;overflow:scroll">
            <asp:GridView ID="gvRequestLog" runat="server">
            </asp:GridView>
        </div>

        <br />
        <br />
        <asp:GridView ID="gvComments" runat="server" EnableModelValidation="True" 
            onrowcommand="gvComments_RowCommand">
            <Columns>
                <asp:ButtonField CommandName="revert" Text="Revert" />
            </Columns>
        </asp:GridView>

        <br />
        <br />
        <table><tr>
        <td>
            <asp:Panel ID="pnlQuerys" runat="server">
            </asp:Panel>
        </td>
        <td>
            <asp:TextBox ID="txtQuery" runat="server" Height="55px" TextMode="MultiLine" 
                Width="451px"></asp:TextBox><br />
                <asp:Button ID="btnRunQuery" runat="server" onclick="btnRunQuery_Click" 
            Text="Run" />
        &nbsp;<asp:Button ID="btnQuery" runat="server" onclick="btnQuery_Click" 
            Text="Grid" />
            <br />
            <br />
            <asp:Button ID="btnSaveQuery" runat="server" onclick="btnSaveQuery_Click" 
                Text="Save Query" />
            &nbsp;<asp:TextBox ID="txtFileName" runat="server"></asp:TextBox>
        </td>
        <td>
        &nbsp;<asp:GridView ID="gvQueryResults" runat="server">        
        </asp:GridView>
        </td>
        </tr></table>
        <br />
        <asp:Button ID="btnThrowException" runat="server" 
            onclick="btnThrowException_Click" Text="Test Exception Throw" />
        <br />
        <asp:Button ID="btnAutoEnrollAllSubs" runat="server" 
            onclick="btnAutoEnrollAllSubs_Click" Text="Auto Enroll All Subs" />
        <br />
        

    </asp:Panel>
</div>

</asp:Content>
