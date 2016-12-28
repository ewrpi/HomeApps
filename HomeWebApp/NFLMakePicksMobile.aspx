<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NFLMakePicksMobile.aspx.cs" Inherits="HomeWebApp.NFLMakePicksMobile" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/Styles/mobilestyle.css" rel="stylesheet" type="text/css" />
</head>
<body id="mobileBody">
    <form id="form1" runat="server">
    <% string clickEvent = HomeWebApp.Common.RapidFireMode && Request.Headers["User-Agent"].ToLower().Contains("mobile") ? "ontouchstart" : "onclick"; %>

    <img id="floatImage" style="position:absolute;" <%=clickEvent %>="handlePickMade('none', 0)" />
    <div align="center">
        <asp:Label ID="lblWeekText" runat="server"></asp:Label>
    </div>
    <div>
        <div id="homeButton" align="center" style="display:inline-block;cursor:pointer;padding: 20px 10px 0px 0px;width:45%;" <%=clickEvent %>="visitorPickClicked()">
        
            <asp:Label ID="lblVisitorCity" runat="server" Font-Bold="True" Font-Italic="False" 
            Font-Size="XX-Large"></asp:Label>
            &nbsp;
            <asp:Label ID="lblVisitorName" runat="server" Font-Bold="True" 
            Font-Italic="False" Font-Size="XX-Large"></asp:Label>

            <br /><br />
            <asp:Image ID="imgVisitor" runat="server" Width="100%" />
            <br /><br />
        
            <asp:Label ID="lblVisitorStatic" runat="server" Font-Bold="True" Font-Italic="False" 
            Font-Size="XX-Large">Visitor</asp:Label>            
        </div>

        <div id="centerSliver" align="center" style="display:inline-block;vertical-align:top;padding-top:250px;" onclick="showHideControls()" >
            <asp:Label ID="Label1" runat="server" Text="@" Font-Size="50pt"></asp:Label>
        </div>
        
        <div id="visitorButton" align="center" style="display:inline-block;cursor:pointer;padding: 20px 0px 0px 10px;width:45%;" <%=clickEvent %>="homePickClicked()">

            <asp:Label ID="lblHomeCity" runat="server" Font-Bold="True" 
            Font-Italic="False" Font-Size="XX-Large"></asp:Label>
            &nbsp;
            <asp:Label ID="lblHomeName" runat="server" Font-Bold="True" Font-Italic="False" 
                Font-Size="XX-Large"></asp:Label>
            <br /><br />
            <asp:Image ID="imgHome" runat="server" Width="100%" />
            <br /><br />
        
            <asp:Label ID="lblHomeStatic" runat="server" Font-Bold="True" Font-Italic="False" 
            Font-Size="XX-Large">Home</asp:Label>            
        </div>
    </div>

        <asp:Panel ID="pnlHidden" runat="server">
        </asp:Panel>
    
        <div id="navigationControls" style="display:none;">
        <br /><br />
            <asp:LinkButton ID="lnkCancel" runat="server" onclick="lnkCancel_Click">Cancel</asp:LinkButton>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <a href="/NFLPicks.aspx">Back</a>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <span class="jsLink" onclick="runColors()">Run Colors</span>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <span class="jsLink" onclick="runImageFloat()">Run Image Float</span>
            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
            <asp:LinkButton ID="lnkLogOut" runat="server" onclick="lnkLogOut_Click">Log Out</asp:LinkButton>
        </div>
    
    </form>
</body>
</html>
<script type="text/javascript">
    var colors = [];
    colors.push("blue");
    colors.push("green");
    colors.push("yellow");
    colors.push("red");
    colors.push("pink");
    colors.push("orange");
    colors.push("purple");
    colors.push("white");

    var floatImages = [];
    floatImages = document.getElementById("floatImages").value.toString().split(',');

    initializeControls();

    var runningColors = false;

    if (document.getElementById("autoruncolors").value == "True")
        runAnimation();

    function runAnimation() {
        runColors();
        runImageFloat();
    }

    function backgroundWorker(id) {

        if (id == null)
            id = "mobileBody";

        var el = document.getElementById(id);

        var index = Math.floor(Math.random() * colors.length);
        el.style.backgroundColor = colors[index];

        setTimeout("backgroundWorker('" + id + "')", 1000);
    }

    function runColors() {
        backgroundWorker("homeButton");
        setTimeout("backgroundWorker('visitorButton')", 333);
        setTimeout("backgroundWorker()", 666);
        runningColors = true;
    }

    function initializeControls() {
        var height = window.innerHeight || document.body.clientHeight;

        document.getElementById("homeButton").style.height = height + "px";
        document.getElementById("visitorButton").style.height = height + "px";
    }
    
    function visitorPickClicked() {
        var choice = document.getElementById("visitor").value;
        var baseUrl = document.location.toString().split('?')[0];

        handlePickMade(baseUrl + "?choice=" + choice + "&homeOrVisitor=Visitor", 0);
        
    }
    function homePickClicked() {
        var choice = document.getElementById("home").value;
        var baseUrl = document.location.toString().split('?')[0];

        handlePickMade(baseUrl + "?choice=" + choice + "&homeOrVisitor=Home", 0);
    }
    function handlePickMade(redirect, recursionCount) {
        if (recursionCount == 100 || !runningColors) {
            if (redirect != 'none')
                document.location = redirect;
        }
        else {
            if (recursionCount % 3 == 0) {
                setRandomColor("homeButton");
            }
            else if (recursionCount % 3 == 1) {
                setRandomColor("visitorButton");
            }
            else if (recursionCount % 3 == 2) {
                setRandomColor("mobileBody");
            }
            setTimeout('handlePickMade("' + redirect + '", ' + (recursionCount + 1) + ')', 10);
        }
    }
    function setRandomColor(id) {
        var el = document.getElementById(id);
        var index = Math.floor(Math.random() * colors.length);
        el.style.backgroundColor = colors[index];
    }
    function showHideControls() {
        var el = document.getElementById("navigationControls");

        if (el.style.display == 'none') {

            el.style.display = 'block';
        }
        else {

            el.style.display = 'none';
        }
    }

    function runImageFloat() {

        var src = getRandomFloatFile();
        var currentX = getRandomX();
        var endX = getRandomX(currentX);
        var currentY = getRandomY();
        var endY = getRandomY(currentY);
        
        doImageMove(src, currentX, currentY, endX, endY); 
    }

    function doImageMove(src, currentX, currentY, endX, endY) {
        var floatImage = document.getElementById("floatImage");
        floatImage.src = src;
        floatImage.style.top = currentY + "px";
        floatImage.style.left = currentX + "px";

        if (currentX != endX && currentY != endY) {
            if (currentX < endX)
                currentX += 1;
            else
                currentX -= 1;

            if (currentY < endY)
                currentY += 1;
            else
                currentY -= 1;
        }
        else {
            src = getRandomFloatFile();
            endX = getRandomX(currentX);
            endY = getRandomY(currentY);
        }

        setTimeout("doImageMove('" + src + "', " + currentX + ", " + currentY + ", " + endX + ", " + endY + ")", 3);
    }

    function getRandomFloatFile() {
        var index = Math.floor(Math.random() * floatImages.length);
        return '/images/floatImages/' + floatImages[index];
    }
    function getRandomX(currentX) {
        if (currentX != null) {
            if (currentX == 0) {
                return getWidth();
            }
            else if (currentX >= getWidth()) {
                return 0;
            }

            var num = Math.random();
            if (num <= 0.5) {
                return 0;
            }
            else {
                return getWidth();
            }
        }

        return Math.floor(Math.random() * getWidth());
    }
    function getRandomY(currentY) {
        if (currentY != null) {
            if (currentY == 0) {
                return getHeight();
            }
            else if (currentY >= getHeight()) {
                return 0;
            }

            var num = Math.random();
            if (num <= 0.5) {
                return 0;
            }
            else {
                return getHeight();
            }
        }

        return Math.floor(Math.random() * getHeight());
    }
    function getWidth() {
        return (document.getElementById("visitorButton").clientWidth * 2)
            + document.getElementById("centerSliver").clientWidth
            - document.getElementById("floatImage").clientWidth;
    }
    function getHeight() {
        return document.getElementById("visitorButton").clientHeight
            - document.getElementById("floatImage").clientHeight;
    }
    window.addEventListener("load", function () {
        // Set a timeout...
        setTimeout(function () {
            // Hide the address bar!
            window.scrollTo(0, 1);
        }, 0);
    });
    
</script>
