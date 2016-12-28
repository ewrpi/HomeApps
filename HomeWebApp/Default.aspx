<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HomeWebApp._Default" MasterPageFile="~/site.Master"%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script language="javascript" type="text/javascript">
// <![CDATA[

        function test_onclick() {

        }

// ]]>
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div align="center">           
    <table>
        <tr>
            <td>
                <br /><br />
                Your IP address: <asp:Label ID="lbl_IPAddress" runat="server"></asp:Label> 
                <br />
                Remote Host:
                <asp:Label ID="lbl_remoteHost" runat="server"></asp:Label>
                <br />
                <br />
                <fb:like href="http://www.facebook.com/eric.wright.37853" send="true" layout="button_count" width="450" show_faces="false"></fb:like>
                <br />

                <a href="AlbumsListing.aspx">Photo Albums</a><br />
                <a href="Apps.aspx">Download an App!</a>

                <br />
                <br />
                <br />
                <br />

                My lady is on the <a href="http://www.weatherfordlabs.com/resources/sample-types/cuttings">Weatherford website!</a><br /><br />
                <a href="http://www.weatherfordlabs.com/resources/sample-types/cuttings">
                    <img alt="" src="http://www.weatherfordlabs.com/umbraco/imagegen.ashx?image=/media/1485/cuttings%202.jpg" />
                </a>
                <br />

                <iframe width="425" height="350" frameborder="0" scrolling="no" marginheight="0" marginwidth="0" src="https://maps.google.com/maps?f=q&amp;source=s_q&amp;hl=en&amp;geocode=&amp;ll=29.996757,-95.720343&amp;output=embed"></iframe><br />
                <iframe width="425" height="350" src="http://www.youtube.com/embed/Ol8cPQvnWEE" frameborder="0"></iframe><br />
                <iframe width="425" height="350" src="file:///C:/Users/Sana/Desktop/webVideos/2008_0809faizashadi0041.MP4" frameborder="0"></iframe><br />
                
            </td>

            <td width="50px"></td>

            <td valign="top" align="left">
            <br /><br />
                <asp:Panel ID="pnlNoMessage" runat="server">                                
                    Enter a message... anything you want!<br />
                    Who are you? Why are you here? Would you like something new added?<br />
                    Or even simply what's your name? The sky is the limit!!<br /><br />                
                    Message<br />
                    <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Height="198px" 
                        Width="315px"></asp:TextBox><br />
                    <asp:Button ID="btnSubmitMessage" runat="server" Text="Submit" 
                        onclick="btnSubmitMessage_Click" />
                </asp:Panel>
                <asp:Panel ID="pnlHasMessage" runat="server">
                    Welcome back!!<br />
                    Your message:<br />
                    <asp:TextBox ID="txtExistingMessage" runat="server" TextMode="MultiLine" Height="198px" 
                        Width="315px" Enabled="False"></asp:TextBox><br />
                    <asp:Button ID="btnModifyMessage" runat="server" Text="Change" 
                        onclick="btnModifyMessage_Click" />
                </asp:Panel>
            </td>
        </tr>
    </table>        
    </div>
</asp:Content>