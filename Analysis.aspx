<%@ Page Language="C#" MasterPageFile="~/Stoocks.master" Title="Stoocker! - Analysis" AutoEventWireup="true" CodeFile="Analysis.aspx.cs" Inherits="Analysis" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">

<!--
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
-->

    
	
    <div>    
        <table>
        <tr>
        <td style="width: 450px">
            
            <br />
            <asp:Label ID="StockDetailsLabel" runat="server" Text="Label"></asp:Label><br />
            <br />
            <asp:Label ID="PredictionMessageLabel" runat="server" Font-Bold="True" Text="What do you think? Will the price move Up or Down Tomorrow?"></asp:Label><br />
            <asp:ImageButton ID="UpPredictionButton" runat="server" ImageUrl="~/Images/upmod.png"
                OnClick="UpPredictionButton_Click" />
            <asp:ImageButton ID="DownPredictionButton" runat="server" ImageUrl="~/Images/downmod.png"
                OnClick="DownPredictionButton_Click" /><br />
            <asp:Label ID="MessageLabel" runat="server"></asp:Label><br />
            <br />
        
            
                
            <asp:Label ID="NewsMessageLabel" runat="server"></asp:Label><br />
            <br />
            <asp:Label ID="NewsLabel" runat="server" BorderColor="Silver" Font-Italic="False" ForeColor="DimGray"></asp:Label>
            <br />     
        </td>
        
        <td>                    
            <asp:PlaceHolder ID="YahooFinanceBadgeIFramePlaceHolder" runat="server"></asp:PlaceHolder>
        </td>
        
        </tr>
        </table>
        
    </div>
 
</asp:Content>