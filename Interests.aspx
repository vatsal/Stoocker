<%@ Page Language="C#" MasterPageFile="~/Stoocks.master"  Title="Stoocker - Interests" AutoEventWireup="true" CodeFile="Interests.aspx.cs" Inherits="Interests" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">

	
    <div>
    
        <asp:Label ID="InterestsLabel" runat="server" Width="580px"></asp:Label><br />
        &nbsp;<br />
        <asp:CheckBoxList ID="StocksCBL" runat="server" RepeatColumns="8" RepeatDirection="Horizontal">
        </asp:CheckBoxList>
        <br />
        <asp:Button ID="AddButton" runat="server" Text="Save Changes" Width="128px" OnClick="AddButton_Click" />&nbsp;<br />
        <br />
        <asp:Label ID="SaveChangesLabel" runat="server"></asp:Label><br />
        <br />
        <br />
        <br />
        <asp:Label ID="FindStockLabel" runat="server"></asp:Label><br />
        <asp:TextBox ID="FindStockTB" runat="server" Width="125px"></asp:TextBox>
        <asp:Label ID="FindStockMessageLabel" runat="server"></asp:Label><br />
        <asp:Button ID="FindStockButton" runat="server" Text="Find" Width="128px" OnClick="FindStockButton_Click" /><br />
        
        
    </div>

    
    <script type="text/javascript" language="javascript">
    <!--
    // JavaScript File
        
    //-->
    </script>
      
</asp:Content>
