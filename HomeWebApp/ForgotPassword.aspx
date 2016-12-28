<%@ Page Title="" Language="C#" MasterPageFile="~/site.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="HomeWebApp.ForgotPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div style="color:Yellow;">
    <asp:Label ID="Label1" runat="server" Font-Size="X-Large" Text="You can request your login info be sent to your email address."></asp:Label>
    <table>
        <tr>
            <td>Name </td>
            <td>
                <asp:TextBox ID="tbName" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td>Email </td>
            <td>
                <asp:TextBox ID="tbEmail" runat="server"></asp:TextBox></td>
        </tr>
        <tr>
            <td></td>
            <td>
                <asp:Button ID="btnSubmit" runat="server" Text="Send Request" 
                    onclick="btnSubmit_Click" />
            </td>
        </tr>
    </table>
    <asp:Label ID="lblStatus" runat="server" Font-Size="X-Large"></asp:Label>
</div>
</asp:Content>
