<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="StravaRecTester.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        Email:
        <asp:TextBox runat="server" ID="TextBox_Username"></asp:TextBox>
        <asp:TextBox runat="server" ID="TextBox_Password" TextMode="Password"></asp:TextBox>
        <asp:Button runat="server" Text="Submit" />
    </div>
    </form>
</body>
</html>
