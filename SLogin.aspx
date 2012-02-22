<%@ Page Language="C#" MasterPageFile="~/Footer.master" AutoEventWireup="true" Title="Stoocker!" CodeFile="SLogin.aspx.cs" Inherits="SLogin" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">
    <div>
        &nbsp;       
        <table>
            <tr>
                <td style="width: 300px">        
                    <table style="width: 160px; border-top-style: solid; border-right-style: solid; border-left-style: solid; border-bottom-style: solid; border-left-color: #c8d2ff; border-bottom-color: #c8d2ff; border-top-color: #c8d2ff; border-right-color: #c8d2ff;" id="LoginTable">
                        <tr>
                            <td style="width: 60px">
                            </td>
                            <td style="width: 90px">
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 60px">
                                <asp:Label ID="UsernameLabel" runat="server" Text="Username"></asp:Label></td>
                            <td style="width: 90px">
                                <asp:TextBox ID="UsernameTB" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 60px">
                                <asp:Label ID="PasswordLabel" runat="server" Text="Password"></asp:Label></td>
                            <td style="width: 90px">
                                <asp:TextBox ID="PasswordTB" runat="server" TextMode="Password"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td style="width: 60px">
                            </td>
                            <td style="width: 90px">
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 60px">
                            </td>
                            <td style="width: 90px;" align="right">
                                <asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="LoginButton_Click" />
                            </td>
                        </tr>
                    </table>
                    
                    <asp:Label ID="MessageLabel" runat="server" Width="200px"></asp:Label><br />
                       
                    <br />
                    <asp:HyperLink ID="NewUserLink" runat="server" Font-Underline="false" NavigateUrl="~/Registration.aspx" Width="175px">New to Stoocker? Sign Up.</asp:HyperLink>
                    <br />
                    <asp:HyperLink ID="ForgotPasswordLink" runat="server" Font-Underline="false" NavigateUrl="~/ForgotPassword.aspx" Width="150px">Forgot Your Password?</asp:HyperLink>&nbsp;<br />
                    <br />
                    <asp:LinkButton ID="DemoLink" runat="server" Font-Underline="False" ForeColor="Blue" OnClick="DemoLink_Click">View Demonstration</asp:LinkButton><br />
                </td>        
                
                <td align="left" valign="top">                                
                    <asp:LinkButton ID="NewsLink" runat="server" Font-Underline="false" BackColor="#C8D2FF" OnClick="NewsLink_Click">Current News</asp:LinkButton>
                    <br />
                    <br />
                    <asp:Label ID="RecommendedStocksLabel" runat="server" ForeColor="Blue"></asp:Label><br />
                    <asp:Table ID="RecommendedStocksTable" runat="server" BorderColor="Silver" BorderStyle="Solid"
                        BorderWidth="2px" GridLines="Both">
                    </asp:Table>
                    <asp:Label ID="CollaborativeRecommendationsLabel" runat="server"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <br />
        
        </td>   
                <td>                   
                    
                </td>         
            </tr>
        </table>     
    </div>
    <!-- <a href="About.aspx" style="text-decoration:none; font-family:Verdana;" >What About Stoocker.com is so obvious.</a> -->
        
        <asp:Label ID="AboutStoockerLabel" runat="server" Width="250px"></asp:Label><br />
                
</asp:Content>