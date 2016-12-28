<%@ Page Title="" Language="C#" MasterPageFile="~/site.Master" AutoEventWireup="true" CodeBehind="WinLossStats.aspx.cs" Inherits="HomeWebApp.WinLossStats" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div>
    <asp:Label ID="lblTitle" runat="server" Font-Size="XX-Large" ForeColor="Yellow"></asp:Label>
    &nbsp;Size up
    <asp:DropDownList ID="ddlTeam1" runat="server">
    </asp:DropDownList>
&nbsp;and
    <asp:DropDownList ID="ddlTeam2" runat="server">
    </asp:DropDownList>
    .
    <asp:Button ID="btnSumit" runat="server" onclick="btnSumit_Click" Text="Go" />
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <asp:Button ID="btnReset" runat="server" onclick="btnReset_Click" 
        Text="Reset" />
    <br />
</div>
<div>
    <asp:GridView ID="gvWinLossStats" runat="server" BackColor="White" 
        BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="4" 
        EnableModelValidation="True" ForeColor="Black" GridLines="Vertical">
        <AlternatingRowStyle BackColor="White" />
        <FooterStyle BackColor="#CCCC99" />
        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
        <RowStyle BackColor="#F7F7DE" />
        <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
    </asp:GridView>
</div>

</asp:Content>
