<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Apps.aspx.cs" Inherits="HomeWebApp.Apps" MasterPageFile="~/site.Master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


<div align="center">
    <font size="100" color="blue">Available apps for download!!</font>
    </div>
    <%if (HttpContext.Current.Session["apps"] == null) HttpContext.Current.Session["apps"] = string.Empty;
      Response.Write(HttpContext.Current.Session["apps"].ToString());%>

    <table>
    </table>


</asp:Content>
    