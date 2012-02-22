<%@ Page Language="C#" MasterPageFile="~/Footer.master" AutoEventWireup="true"  Title="Stoocker! - Forgot Password" CodeFile="ForgotPassword.aspx.cs" Inherits="ForgotPassword" %>

<asp:Content ID="Main" ContentPlaceHolderID="Main" runat="server">              
    <div>
        <br />
        <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" BackColor="White" BorderColor="#B5C7DE"
            BorderPadding="4" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana"
            Font-Size="0.8em" Width="240px">
            <InstructionTextStyle Font-Italic="True" ForeColor="Black" />
            <SuccessTextStyle Font-Bold="True" ForeColor="#507CD1" />
            <TextBoxStyle Font-Size="0.8em" />
            <TitleTextStyle BackColor="#507CD1" Font-Bold="True" Font-Size="0.9em" ForeColor="White" />
            <SubmitButtonStyle BackColor="White" BorderColor="#507CD1" BorderStyle="Solid" BorderWidth="1px"
                Font-Names="Verdana" Font-Size="0.8em" ForeColor="#284E98" />
            <UserNameTemplate>
                <table border="0" cellpadding="4" cellspacing="0" style="border-collapse: collapse; width: 230px;">
                    <tr>
                        <td style="width: 240px">
                            <table border="0" cellpadding="0">
                                <tr>
                                    <td align="center" colspan="2" style="font-weight: bold; font-size: 0.9em; color: white;
                                        background-color: #507cd1">
                                        Forgot Your Password?</td>
                                </tr>
                                <tr>
                                    <td align="center" colspan="2" style="color: black; font-style: italic">
                                        </td>
                                </tr>
                                <tr>
                                    <td align="right" style="width: 80px">
                                        <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">User Name:</asp:Label></td>
                                    <td>
                                        <asp:TextBox ID="UserName" runat="server" Font-Size="0.8em"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                                            ErrorMessage="User Name is required." ToolTip="User Name is required." ValidationGroup="PasswordRecovery1">*</asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="center" colspan="2" style="color: red">
                                        <asp:Label ID="ErrorLabel" runat="server"></asp:Label></td>
                                </tr>
                                <tr>
                                    <td align="right" colspan="2">
                                        <asp:Button ID="SubmitButton" runat="server" BackColor="White" BorderColor="#507CD1"
                                            BorderStyle="Solid" BorderWidth="1px" CommandName="Submit" Font-Names="Verdana"
                                            Font-Size="0.8em" ForeColor="#284E98" Text="Submit" ValidationGroup="PasswordRecovery1" OnClick="SubmitButton_Click" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </UserNameTemplate>
        </asp:PasswordRecovery>
        <br />
        <asp:HyperLink ID="LoginLink" runat="server"  Font-Underline="false" NavigateUrl="~/SLogin.aspx">Return to the Login Page</asp:HyperLink></div>
    
  </asp:Content>  

