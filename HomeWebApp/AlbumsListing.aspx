<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AlbumsListing.aspx.cs" Inherits="HomeWebApp.AlbumsListing" MasterPageFile="~/site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div align="center">

    <font size="100" color="blue">Current photo albums</font>

    <asp:GridView ID="gv_albums" runat="server" AutoGenerateColumns="False" 
            EnableModelValidation="True" onrowdatabound="gv_albums_RowDataBound" 
            onrowcommand="gv_albums_RowCommand" BackColor="White" 
            BorderColor="#CC9966" BorderStyle="None" BorderWidth="1px" CellPadding="4">
        <Columns>
            <asp:HyperLinkField DataTextField="Albums" HeaderText="Albums" />
            <asp:ButtonField HeaderText="Should I resize?" Text="Go" CommandName="select" />
            <asp:ImageField DataImageUrlField="image_url">
            </asp:ImageField>
        </Columns>
        <FooterStyle BackColor="#FFFFCC" ForeColor="#330099" />
        <HeaderStyle BackColor="#990000" Font-Bold="True" ForeColor="#FFFFCC" />
        <PagerStyle BackColor="#FFFFCC" ForeColor="#330099" HorizontalAlign="Center" />
        <RowStyle BackColor="LightBlue" ForeColor="#330099" />
        <SelectedRowStyle BackColor="#FFCC66" Font-Bold="True" ForeColor="#663399" />
        </asp:GridView>

        <asp:Label ID="lbl_status" runat="server"></asp:Label>
    </div>

</asp:Content>