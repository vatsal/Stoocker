<%@ Page Language="C#" MasterPageFile="~/Footer.master" AutoEventWireup="true" Title="Stoocker! - News" CodeFile="News.aspx.cs" Inherits="News" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">    
    <div id="NewsSearchDiv" style="background-color:#f5f5f5">
    <table>
        <tr>
            <td style="width: 650px; height: 50px">
                <asp:Label ID="NewsSearchLabel" runat="server" Width="100px">Enter a Symbol</asp:Label>
                <asp:TextBox ID="NewsSearchTB" runat="server" Height="15px" Width="210px"></asp:TextBox>
                <asp:Button ID="NewsSearchButton" runat="server" Text="Get News" OnClick="NewsSearchButton_Click"></asp:Button>
                <asp:Label ID="NewsSearchMessageLabel" runat="server"></asp:Label><br />
            </td>
            <td align="right">
                <asp:LinkButton ID="GoBackLink" runat="server"  Font-Underline="false" BackColor="#c8d2ff" OnClick="GoBackLink_Click"></asp:LinkButton>                                                
            </td>
            <td align="right">
                <asp:LinkButton ID="LogoutLink" runat="server"  Font-Underline="false" BackColor="#c8d2ff" OnClick="LogoutLink_Click">Logout</asp:LinkButton>                                                
            </td>
        </tr>
    </table>
    
    <table>
        <tr>            
            <td align="right">
                <asp:LinkButton ID="TopHeadlinesLink" runat="server"  Font-Underline="false" BackColor="#c8d2ff" OnClick="TopHeadlinesLink_Click">Top Headlines</asp:LinkButton>
            </td>
            <td align="right">
                <asp:LinkButton ID="IPOLink" runat="server"  Font-Underline="false" BackColor="#c8d2ff" OnClick="IPOLink_Click">IPO News</asp:LinkButton>
            </td>                    
        </tr>
    </table>
        
    </div>    
    <!-- <table style="width:auto; height:auto; border-right: silver thin solid; border-top: silver thin solid; border-left: silver thin solid; border-bottom: silver thin solid;"> -->
    <br />
    <asp:Table ID="StockTable" runat="server" BackColor="Transparent" BorderColor="Black"
        BorderStyle="Double" GridLines="Both" Height="23px" Width="250px">
    </asp:Table>
    <br /> 
    <table style="width:auto; height:auto;">
        <tr>
            <td>
                <div id="NewsDivA">
                    <asp:Label ID="NewsMessageLabelA" runat="server"></asp:Label><br />
                    <br />
                    <asp:Label ID="NewsLabelA" runat="server"></asp:Label>
                </div>
            </td>
            <td>
                <div id="NewsDivB">
                    <asp:Label ID="NewsMessageLabelB" runat="server"></asp:Label><br />
                    <br />
                    <asp:Label ID="NewsLabelB" runat="server"></asp:Label>
                </div>
            </td>
        </tr>
        
        <tr>
            <td>
                <br />
            </td>
            <td></td>
        </tr>
        <tr>
            <td>
               <div id="NewsDivC">
                   <asp:Label ID="NewsMessageLabelC" runat="server"></asp:Label><br />
                   <br />
                   <asp:Label ID="NewsLabelC" runat="server"></asp:Label>
               </div> 
            </td>
            <td>
               <div id="NewsDivD">
                   <asp:Label ID="NewsMessageLabelD" runat="server"></asp:Label><br />
                   <br />
                   <asp:Label ID="NewsLabelD" runat="server"></asp:Label>
               </div> 
            </td>
        </tr>
        
        <tr>
            <td>
                <br />
            </td>
            <td></td>            
        </tr>
        <tr>
            <td>
                <asp:Label ID="NewsMessageLabelE" runat="server"></asp:Label><br />
                <br />
                <asp:Label ID="NewsLabelE" runat="server"></asp:Label></td>
            <td>
                <asp:Label ID="NewsMessageLabelF" runat="server"></asp:Label><br />
                <br />
                <asp:Label ID="NewsLabelF" runat="server"></asp:Label><td>
                
            </td>            
        </tr>
        
        <tr>
            <td>
            
            </td>
            <td align="right">
                <br />
                <asp:Label ID="NewsRefreshRateLabel" runat="server"></asp:Label>                
            </td>            
        </tr>        
    </table>
</asp:Content>