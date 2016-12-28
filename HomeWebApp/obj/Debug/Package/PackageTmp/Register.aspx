<%@ Page Title="" Language="C#" MasterPageFile="~/site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="HomeWebApp.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<table>
    <tr align="left">
        <td class="fieldLabel">Name: </td>
        <td>
            <asp:TextBox ID="txtName" runat="server"></asp:TextBox></td>
    </tr>
    <tr align="left">
        <td class="fieldLabel">Email address (optional): </td>
        <td>
            <asp:TextBox ID="txtEmailAddress" runat="server"></asp:TextBox></td>
    </tr>
    <tr align="left" class="fieldLabel">
        <td class="fieldLabel">Birthday (optional): </td>
        <td>
            <asp:TextBox ID="txtBirthday" runat="server"></asp:TextBox>
        </td>
    </tr>

    <tr align="left">
        <td class="fieldLabel">Password: </td>
        <td>
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox></td>
    </tr>
    <tr align="left">
        <td class="fieldLabel">Confirm password: </td>
        <td><asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password"></asp:TextBox></td>
    </tr>
    <tr align="left">
        <td></td>
        <td>
            <asp:Button ID="btnSubmit" runat="server" Text="Submit" 
                onclick="btnSubmit_Click" /></td>
    </tr>
</table><br />
    <asp:Label ID="lblerror" runat="server" Font-Bold="True" Font-Size="X-Large" 
        ForeColor="Yellow"></asp:Label><br /><br /><br />
    <asp:Label ID="lblCurrentUserList" runat="server"></asp:Label>

</asp:Content>
