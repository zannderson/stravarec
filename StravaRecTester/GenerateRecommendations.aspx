<%@ Page Async="true" AsyncTimeout="1200" Language="C#" AutoEventWireup="true" CodeBehind="GenerateRecommendations.aspx.cs" Inherits="StravaRecTester.GenerateRecommendations" %>

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
        <asp:Panel runat="server" ID="Panel_Results" Visible="false">
            <div class="group-label">Uphill Segment Recommendations</div>
            <asp:Repeater ID="Repeater_Uphill" runat="server">
                <ItemTemplate>
                    <asp:Panel runat="server" ID="Panel_Result" CssClass="result-row">
                        <div class="name-div result-cell"><asp:Label id="Literal_SegmentName" runat="server" Text='<%# Eval("Name") %>' CssClass="result-name"></asp:Label></div>
                        <div class="view-seg-div result-cell"><asp:HyperLink ID="HyperLink_SegmentPage" runat="server" NavigateUrl='<%# Eval("Url") %>' CssClass="result-link">View Segment</asp:HyperLink></div>
                        <div class="user-rank-div result-cell"><asp:Label ID="Label_UserRank" runat="server" CssClass="user-rank-label" Text="Your Ranking:"></asp:Label>
                        <asp:TextBox ID="TextBox_UserRank" runat="server" CssClass="result-user-rating" ></asp:TextBox></div>
                        <div class="would-you-pick-div result-cell"><asp:CheckBox ID="CheckBox_WouldYouPick" runat="server" CssClass="would-you-pick-checkbox" Text="Would you pick this segment?" /></div>
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>
            <div class="group-label">Downhill Segment Recommendations</div>
            <asp:Repeater ID="Repeater_Downhill" runat="server">
                <ItemTemplate>
                    <asp:Panel runat="server" ID="Panel_Result" CssClass="result-row">
                        <div class="name-div result-cell"><asp:Label id="Literal_SegmentName" runat="server" Text='<%# Eval("Name") %>' CssClass="result-name"></asp:Label></div>
                        <div class="view-seg-div result-cell"><asp:HyperLink ID="HyperLink_SegmentPage" runat="server" NavigateUrl='<%# Eval("Url") %>' CssClass="result-link">View Segment</asp:HyperLink></div>
                        <div class="user-rank-div result-cell"><asp:Label ID="Label_UserRank" runat="server" CssClass="user-rank-label" Text="Your Ranking:"></asp:Label>
                        <asp:TextBox ID="TextBox_UserRank" runat="server" CssClass="result-user-rating" ></asp:TextBox></div>
                        <div class="would-you-pick-div result-cell"><asp:CheckBox ID="CheckBox_WouldYouPick" runat="server" CssClass="would-you-pick-checkbox" Text="Would you pick this segment?" /></div>
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>
            <div class="group-label">Flat Segment Recommendations</div>
            <asp:Repeater ID="Repeater_Flat" runat="server">
                <ItemTemplate>
                    <asp:Panel runat="server" ID="Panel_Result" CssClass="result-row">
                        <div class="name-div result-cell"><asp:Label id="Literal_SegmentName" runat="server" Text='<%# Eval("Name") %>' CssClass="result-name"></asp:Label></div>
                        <div class="view-seg-div result-cell"><asp:HyperLink ID="HyperLink_SegmentPage" runat="server" NavigateUrl='<%# Eval("Url") %>' CssClass="result-link">View Segment</asp:HyperLink></div>
                        <div class="user-rank-div result-cell"><asp:Label ID="Label_UserRank" runat="server" CssClass="user-rank-label" Text="Your Ranking:"></asp:Label>
                        <asp:TextBox ID="TextBox_UserRank" runat="server" CssClass="result-user-rating" ></asp:TextBox></div>
                        <div class="would-you-pick-div result-cell"><asp:CheckBox ID="CheckBox_WouldYouPick" runat="server" CssClass="would-you-pick-checkbox" Text="Would you pick this segment?" /></div>
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>
            <div class="group-label">Rolling/Up and Down Segment Recommendations</div>
            <asp:Repeater ID="Repeater_Rolling" runat="server">
                <ItemTemplate>
                    <asp:Panel runat="server" ID="Panel_Result" CssClass="result-row">
                        <div class="name-div result-cell"><asp:Label id="Literal_SegmentName" runat="server" Text='<%# Eval("Name") %>' CssClass="result-name"></asp:Label></div>
                        <div class="view-seg-div result-cell"><asp:HyperLink ID="HyperLink_SegmentPage" runat="server" NavigateUrl='<%# Eval("Url") %>' CssClass="result-link">View Segment</asp:HyperLink></div>
                        <div class="user-rank-div result-cell"><asp:Label ID="Label_UserRank" runat="server" CssClass="user-rank-label" Text="Your Ranking:"></asp:Label>
                        <asp:TextBox ID="TextBox_UserRank" runat="server" CssClass="result-user-rating" ></asp:TextBox></div>
                        <div class="would-you-pick-div result-cell"><asp:CheckBox ID="CheckBox_WouldYouPick" runat="server" CssClass="would-you-pick-checkbox" Text="Would you pick this segment?" /></div>
                    </asp:Panel>
                </ItemTemplate>
            </asp:Repeater>
            <div class="feedback-submit">
                <asp:Label ID="Label_Feedback" runat="server" Text="Please leave any comments or feedback you have. Thank you."></asp:Label>
                <div class="feedback-box">
                    <asp:TextBox ID="TextBox_Feedback" runat="server" Rows="4" ></asp:TextBox>
                </div>
                <asp:Button ID="Button_SubmitMyRankings" runat="server" CssClass="button-submit" Text="Submit my Rankings" OnClick="Button_SubmitMyRankings_Click" />
            </div>
        </asp:Panel>
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
