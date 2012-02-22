<%@ Page Language="C#" MasterPageFile="~/Stoocks.master" Title="Stoocker! - Home" AutoEventWireup="true" CodeFile="Home.aspx.cs" Inherits="Home" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">

    <div>                    
        
        <table>
            <tr>
                <td style="height: 100px; width: 80%">
                    <!-- <div id="UserDiv" visible="true" style="border-style:solid; border-color:Silver;">        -->
            
                    <div id="UserDiv">
                    <asp:Label ID="WelcomeLabel" runat="server" Width="750px"></asp:Label><br />
                    <asp:Label ID="UserDetailsLabel" runat="server" Height="25px" Width="750px"></asp:Label>&nbsp;</div>
                </td>                        
                <td style="height: 100px; width: 20%">                   
                    &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;                    
                    <asp:Label ID="IndicesTimeLabel" runat="server"></asp:Label>                   
                    <asp:Table ID="CommonIndicesTable" runat="server" Height="50px" Width="50px" GridLines="Both" BackColor="Transparent" BorderColor="Black" BorderStyle="Double" HorizontalAlign="Right"></asp:Table>                                                            
                </td>   
            </tr>        
        </table>
        
        <div id="StockDiv" visible="true" style="border-style:solid; border-color:#EFF3FB;">
            <br />
            <asp:Label ID="PortfolioLabel" runat="server"></asp:Label>
            <br />           
                        
            <table>
                <tr>
                    <td>
                        <table>
                            <tr align="right">
                                <td style="height: 40px">
                                <asp:Label ID="MessageLabel" runat="server"></asp:Label><br />
                                    <br />
                                    <asp:Label ID="TimeLabel" runat="server"></asp:Label>
                                    &nbsp; 
                                    <asp:HyperLink ID="RefreshLink" runat="server" BackColor="#C8D2FF" Font-Underline="False" ForeColor="Black" NavigateUrl="~/Home.aspx" >Refresh Page</asp:HyperLink>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                <asp:Table ID="StockTable" runat="server" GridLines="Both" Height="100px" Width="250px" BackColor="Transparent" BorderColor="Black" BorderStyle="Double"></asp:Table>
                                    <asp:Label ID="ExplanationLabel" runat="server"></asp:Label><br />
                                <br />                                
                                </td>
                            </tr>
                        </table>
                        
                        <asp:Label ID="NewsMessageLabel" runat="server"></asp:Label><br />
                        <br />        
                        <asp:Label ID="NewsLabel" runat="server" BorderColor="WhiteSmoke" Font-Italic="False" ForeColor="DimGray"></asp:Label><br />
                        <br />
                    </td>
                        
                    <td valign="top" align="right">
                        <asp:PlaceHolder ID="YahooFinanceBadgeIFramePlaceHolder" runat="server"></asp:PlaceHolder>
                        <%-- <YahooFinanceBadge:YahooFinanceBadge runat="Server" ID="yChart" /> --%>
                    </td>
                </tr>
            </table>           
        </div>
        <br />
    </div>  
</asp:Content>