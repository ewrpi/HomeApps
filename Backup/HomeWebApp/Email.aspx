<%@ Page Title="" Language="C#" MasterPageFile="~/site.Master" AutoEventWireup="true" CodeBehind="Email.aspx.cs" Inherits="HomeWebApp.Email" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div align="left">
    <asp:Label ID="lblMessageFromQueryString" runat="server" Font-Bold="True" 
        Font-Size="X-Large" ForeColor="Yellow"></asp:Label><br />
    <asp:TextBox ID="txtEmailBody" runat="server" TextMode="MultiLine" 
        Height="150px" Width="422px"></asp:TextBox><br />
    <asp:Button ID="btnSendEmail" runat="server" Text="Send" 
        onclick="btnSendEmail_Click" />
&nbsp;<asp:Label ID="lblSendStatus" runat="server" Font-Bold="True" 
        Font-Size="X-Large" ForeColor="Yellow"></asp:Label>
</div>

</asp:Content>
