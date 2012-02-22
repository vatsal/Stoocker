<%@ Page Language="C#" MasterPageFile="~/Stoocks.master" Title="Stoocker! - User Prediction History" AutoEventWireup="true" CodeFile="UserPredictionHistory.aspx.cs" Inherits="UserPredictionHistory" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">

    <div>
    
        
        <asp:Label ID="UserDetailsLabel" runat="server" Height="50px" Width="750px"></asp:Label><br />
                        
        <table>
        <tr>
        <td style="width:300">
            <asp:Label ID="StockLabel" runat="server" Text=""></asp:Label>
            <br />
            <asp:Table ID="StockTable" runat="server" GridLines="Both" Height="25px" Width="25px"></asp:Table>
        </td>
        
        <td align="center" style="width:300">     
            <asp:Label ID="StoockerAccuracyLabel" runat="server" Text=""></asp:Label>
            <br />       
            <asp:Table ID="StoockerAccuracyTable" runat="server" GridLines="Both" Height="25px" Width="25px"></asp:Table>
        </td>
        
        </tr>
        </table>                 
        
                
    </div>


</asp:Content>