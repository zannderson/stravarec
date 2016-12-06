<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="GenerateRecommendations.aspx.cs" Inherits="StravaRecTester.GenerateRecommendations" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="maindiv">
        <asp:Button runat="server" ID="DoIt" Text="Generate New Recommendations" OnClick="DoIt_Click" Visible="false"/>
        <asp:Literal runat="server" ID="Literal_KeepWaiting" Text="Recommendations are being generated. You will receive an email with a link when they are ready..."></asp:Literal>
        <asp:Literal runat="server" ID="Literal_Done" Text="DONE!!!" Visible="false"></asp:Literal>
    </div>
    </form>
    <script type="text/javascript" src="Scripts/jquery-1.10.2.js" defer="defer" ></script>
    <script type="text/javascript" src="Scripts/spin.js" defer="defer"></script>
    <script type="text/javascript" defer="defer" >
        function clientfunction() {
            var spinner = new Spinner().spin();
            $("#maindiv").append(spinner.el);
        }
    </script>
</body>
</html>
