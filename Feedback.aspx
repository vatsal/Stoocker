<%@ Page Language="C#" MasterPageFile="~/Footer.master" AutoEventWireup="true" Title="Stoocker! - Feedback" CodeFile="Feedback.aspx.cs" Inherits="Feedback" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">
    <div>
        <asp:Label ID="MessageLabel" runat="server"></asp:Label><br />
        <br />
        <asp:Label ID="RequiredFieldsLabel" runat="server" Text="Please Fill Out All the Fields below."></asp:Label><br />
        <br />
        <table style="width: 750px">
            <tr>
                <td style="width: 170px">
                    <asp:Label ID="NameLabel" runat="server" Text="Your Name"></asp:Label></td>
                <td style="width: 190px">
                    <asp:TextBox ID="NameTB" runat="server"></asp:TextBox></td>
            </tr>
            <tr>
                <td style="width: 170px">
                    <asp:Label ID="EmailLabel" runat="server" Text="Your E-Mail Address"></asp:Label></td>
                <td style="width: 190px">
                    <asp:TextBox ID="EMailTB" runat="server"></asp:TextBox>&nbsp;<asp:Label ID="NoSpamLabel"
                        runat="server" Text="[No Spamming, that's a Promise!]" Width="212px"></asp:Label></td>
            </tr>
            <tr>
                <td style="width: 170px; height: 24px">
                    <asp:Label ID="TopicLabel" runat="server" Text="What are you writing about"></asp:Label></td>
                <td style="width: 190px; height: 24px">
                    <asp:DropDownList ID="TopicDDL" runat="server" Width="200px">
                        <asp:ListItem>New Feature Requests</asp:ListItem>
                        <asp:ListItem>Help and Support</asp:ListItem>
                        <asp:ListItem>Bugs/Errors </asp:ListItem>
                        <asp:ListItem>Praise and Flowers</asp:ListItem>
                        <asp:ListItem>Rotten Tomatoes and Stones</asp:ListItem>
                        <asp:ListItem>Go On A Date with Vatsal</asp:ListItem>
                    </asp:DropDownList></td>
            </tr>
            <tr>
                <td style="width: 170px; height: 150px;">
                    <asp:Label ID="FeedbackLabel" runat="server" Text="Feedback"></asp:Label></td>
                <td style="width: 190px; height: 150px;">
                    <asp:TextBox ID="FeedbackTB" runat="server" Height="125px" TextMode="MultiLine" Width="350px"></asp:TextBox></td>
            </tr>
            <tr>
                <td style="width: 170px">
                </td>
                <td style="width: 190px">
                </td>
            </tr>
            <tr>
                <td style="width: 170px; height: 26px">
                </td>
                <td style="width: 190px; height: 26px">
                    <asp:Button ID="SubmitFeedbackButton" runat="server" Text="Submit Feedback" OnClick="SubmitFeedbackButton_Click" /></td>
            </tr>
        </table>
    </div>
    <br />
    <asp:Label ID="OutputLabel" runat="server" Text="..."></asp:Label><br />
</asp:Content>