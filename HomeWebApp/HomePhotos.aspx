<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomePhotos.aspx.cs" Inherits="HomeWebApp.HomePhotos" MasterPageFile="~/site.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div align="left">
    <table><tr>
    <td align="left" valign="top" width="300px">    
        
        <a href="AlbumsListing.aspx">Back to Albums</a><br />
        <a href="Default.aspx">Back Home</a><br />
         <table>
            <tr>      
                <td>All Pictures: </td>
                <td><asp:Button ID="btn_all" runat="server" Text="Show" onclick="btn_all_Click" ></asp:Button>
                </td>

                <td><asp:Button ID="btn_undoAll" runat="server" Text="Hide" 
                        onclick="btn_undoAll_Click" />
                </td>
            </tr>
        </table><br />

        <table><tr>
        <td align="left" valign="top">

            <asp:Button ID="btn_last" runat="server" Text=" << "    
                            onclick="btn_last_Click" Height="112px" Width="63px" ></asp:Button>
        </td>
        <td align="right" valign="top">
            &nbsp;<asp:Button ID="btn_next" runat="server" Text=" >> " 
                            onclick="btn_next_Click" Height="112px" Width="63px"></asp:Button>
        </td>
        </tr></table>

    </td>
    <td align="left">
                

                <asp:ImageButton ID="img_currentImage" runat="server" OnClientClick="aspnetForm.target ='_blank';"
                    onclick="img_currentImage_Click" Width="600px"></asp:ImageButton>
         
                    
    </td>

    <td align="left" valign="top">
        Current cover: 
        <asp:Label ID="lblCover" runat="server"></asp:Label><br />

        <asp:Image ID="imgCover" runat="server" Height="100px" /><br /><br />

        <asp:Button ID="btnSetCurrentPicToCover" runat="server" 
            Text="Set current picture as album cover" 
            onclick="btnSetCurrentPicToCover_Click" />

    </td>
    </tr></table>
</div>
    
<div align="center">
        <%if (HttpContext.Current.Session["all"] == null) HttpContext.Current.Session["all"] = string.Empty; %>
        <%Response.Write(HttpContext.Current.Session["all"].ToString()); %>
</div>   
    

</asp:Content>