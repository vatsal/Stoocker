<%@ Page Language="C#"  MasterPageFile="~/Footer.master" AutoEventWireup="true"  Title="Stoocker! - About" CodeFile="About.aspx.cs" Inherits="About" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">
    <div>
        <table>
            <tr>
                <td>
                    <asp:Label ID="AboutLabel" runat="server">        </asp:Label>    
                </td>
            </tr>

            <tr>
                <td>
                    <asp:Label ID="ExplanationLabel" runat="server">        </asp:Label>    
                </td>
            </tr>
            
            <tr>
                <td>
                    <asp:Label ID="AdvantageLabel" runat="server">        </asp:Label>    
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
