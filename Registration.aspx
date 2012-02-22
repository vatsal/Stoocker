<%@ Page Language="C#" MasterPageFile="~/Footer.master" AutoEventWireup="true"  Title="Stoocker! - Registration" CodeFile="Registration.aspx.cs" Inherits="Registration" EnableEventValidation="true" %>
<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">
    <div>   
    <table>
    <tr>
    <td>
        <table style="font-size: 100%; font-family: 'Times New Roman'; border-left-color: #c8d2ff; border-bottom-color: #c8d2ff; border-top-style: solid; border-top-color: #c8d2ff; border-right-style: solid; border-left-style: solid; border-right-color: #c8d2ff; border-bottom-style: solid;" id="RegistrationTable">
            <tr>
                <td align="center" colspan="2" style="font-weight:normal; color: black; height: 14px;">
                    Sign Up for Your New Account
                </td>
            </tr>
            <tr>
                <td align="right">
                    <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label></td>
                <td>
                    <asp:TextBox ID="UserName" runat="server" MaxLength="20" Width="150px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                        ErrorMessage="User Name is required." ToolTip="User Name is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">Password:</asp:Label></td>
                <td>
                    <asp:TextBox ID="Password" runat="server" MaxLength="20" TextMode="Password" Width="150px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                        ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <asp:Label ID="ConfirmPasswordLabel" runat="server" AssociatedControlID="ConfirmPassword">Confirm Password:</asp:Label></td>
                <td>
                    <asp:TextBox ID="ConfirmPassword" runat="server" MaxLength="20" TextMode="Password"
                        Width="150px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" ControlToValidate="ConfirmPassword"
                        ErrorMessage="Confirm Password is required." ToolTip="Confirm Password is required."
                        ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td align="right">
                    <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="Email">E-mail:</asp:Label></td>
                <td>
                    <asp:TextBox ID="Email" runat="server" Width="150px"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email"
                        ErrorMessage="E-mail is required." ToolTip="E-mail is required." ValidationGroup="CreateUserWizard1">*</asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td align="right" style="height: 25px">
                    <asp:Label ID="ExpertiseLabel" runat="server" AssociatedControlID="ExpertiseDDL" Visible="False">Expertise in Stock Prediction:</asp:Label></td>
                <td style="height: 25px">
                    <asp:DropDownList ID="ExpertiseDDL" runat="server" Width="155px" Visible="False">
                        <asp:ListItem Value="1">Beginner</asp:ListItem>
                        <asp:ListItem Value="2">Intermediate</asp:ListItem>
                        <asp:ListItem Value="3">Expert</asp:ListItem>
                        <asp:ListItem Value="4">Master</asp:ListItem>
                    </asp:DropDownList></td>
            </tr>
            <tr>
                <td align="right" style="height: 25px">
                </td>
                <td style="height: 25px">
                    &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                    <asp:Button ID="RegistrationButton" runat="server" OnClick="RegistrationButton_Click" Text="Register" /></td>
            </tr>
            <tr>
                <td align="center" colspan="2" style="height: 30px">
                    <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password"
                        ControlToValidate="ConfirmPassword" Display="Dynamic" ErrorMessage="The Password and Confirmation Password must match."
                        ValidationGroup="CreateUserWizard1"></asp:CompareValidator>
                </td>
            </tr>
            <tr>
                <td align="center" colspan="2" style="color: red">
                    <asp:Label ID="ErrorLabel" runat="server"></asp:Label></td>
            </tr>
        </table>


        &nbsp;&nbsp;
        <br />
        <asp:HyperLink ID="LoginLink" runat="server" Font-Underline="false" NavigateUrl="~/SLogin.aspx">Already have a Stoocker Account?</asp:HyperLink><br />
        &nbsp;&nbsp;
        </td>
        <td valign="top">
            <br />        
            <br />        &nbsp;</td>
        
         </tr>
    </table> 
            <asp:Label id="AboutStoockerLabel" runat="server"></asp:Label><br />
    </div>
    
   
  
</asp:Content>