using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Security.Cryptography;
using System.Text;
using System.Net;
using MySql.Data.MySqlClient;


public partial class ForgotPassword : System.Web.UI.Page
{
    DBOperations dbOps;
    Logger log;
    Links links;
    General general;

    int sessionTimeoutMinutes = int.Parse(ConfigurationManager.AppSettings["sessionTimeoutMinutes"]);
    int isDoPasswordHashing = int.Parse(ConfigurationManager.AppSettings["isDoPasswordHashing"]);

    string stoockerMail = ConfigurationManager.AppSettings["stoockerMail"];

    private string smtpClient = ConfigurationManager.AppSettings["smtpClient"];


    static Label ErrorLabel;
    
    
    protected void Page_Load(object sender, EventArgs e)
    {
        dbOps = (DBOperations)Application["dbOps"];
        log = (Logger)Application["log"];
        links = (Links)Application["links"];
        general = (General)Application["general"];
        
    }

    protected void SubmitButton_Click(object sender, EventArgs e)
    {
        ErrorLabel = (Label)PasswordRecovery1.UserNameTemplateContainer.FindControl("ErrorLabel");

        string username = PasswordRecovery1.UserName.Trim();
        string message = "";

        if (username.Length >= 4)
        {
            string userMailID = GetUserMailID(username);
            if (userMailID != "-1" && userMailID != "")
            {
                //  Reset the Password.
                string newPassword = GenerateRandomPassword();

                //  EMail the New Password to the User.
                string subject = "Message from Stoocker.com";
                string userMessage = "Your Password has been reset. The new Password is: " + newPassword;
                bool isMailed = general.SendMail(stoockerMail, userMailID, subject, userMessage);
                if (isMailed)
                {
                    //  Hash the Password
                    string passwordHash = newPassword;
                    if (isDoPasswordHashing == 1)
                    {
                        passwordHash = dbOps.HashPassword(newPassword);
                    }

                    //  Reset the Password in the Database.
                    ResetPassword(username, passwordHash);
                    message = "Your Password has been E-Mailed.";
                }
                else
                {
                    message = "An error occurred while E-Mailing the Password";
                }
            }
            else
            {
                message = "No such Username exists for Stoocker.com";
            }
        }
        else
        {
            message = "All UserID's are atleast 4 characters long.";
        }
        ErrorLabel.Text = message;
    }

    protected void PasswordRecovery1_SendingMail(object sender, MailMessageEventArgs e)
    {
        //ErrorLabel = (Label)PasswordRecovery1.FindControl("ErrorLabel");

        //string username = PasswordRecovery1.UserName.Trim();
        //string message = "";

        //if (username.Length >= 4)
        //{
        //    string userMailID = GetUserMailID(username);
        //    if (userMailID != "-1" && userMailID != "")
        //    {
        //        //  Reset the Password.
        //        string newPassword = GenerateRandomPassword();

        //        //  EMail the New Password to the User.
        //        string subject = "Message from Stoocks";
        //        string userMessage = "Your Password has been reset. The new Password is: " + newPassword;
        //        SendMail("vatsals@vatsals.com", userMailID, subject, userMessage);

        //        //  Hash the Password
        //        string passwordHash = HashPassword(newPassword);

        //        //  Reset the Password in the Database.
        //        ResetPassword(username, passwordHash);
        //        message = "Your Password has been E-Mailed.";
        //    }
        //    else
        //    {
        //        message = "No such Username exists for Stoocks.";
        //    }
        //    ErrorLabel.Text = message;
        //}
    }

    private string GetUserMailID(string username)
    {
        string eMail = "";
        string queryString = @"SELECT EMail FROM stoocks.registration WHERE Username='" + username + "' ;";

        MySqlDataReader retList = dbOps.ExecuteReader(queryString);
        if(retList != null && retList.HasRows)
            while (retList.Read())
            {
                eMail = retList.GetString(0);
            }
        retList.Close();
        return eMail;
    }

    private string GenerateRandomPassword()
    {        
        StringBuilder randomPassword = new StringBuilder();
        Random random = new Random();
        char ch;

        int size = random.Next(4, 12);

        for (int i = 0; i < size; i++)
        {
            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
            randomPassword.Append(ch);
        }       
        return randomPassword.ToString();       
    }
         
    private int ResetPassword(string username, string passwordHash)
    {
        //  DBOperations dbOps = new DBOperations();

        //  Update the Registration Table;
        string queryString = @"UPDATE stoocks.registration SET Password = '" + passwordHash + "' WHERE Username='" + username + "';";
        int retVal = dbOps.ExecuteNonQuery(queryString);

        if (retVal == 1)    
        {
                //  Update the Login Table;
            queryString = @"UPDATE stoocks.login SET Password = '" + passwordHash + "' WHERE Username='" + username + "';"; ;
            retVal = dbOps.ExecuteNonQuery(queryString);
            if (retVal == 1)
            {

            }
            else
            {
                retVal = -1;
            }
        }
        else    //  An error occurred.
        {
            retVal = -1;
        }
        return retVal;
    }

    
    #region Login Methods (Not In Use Since December 2007)
    /*
    protected void LoginButton_Click(object sender, EventArgs e)
    {
        string stoocksUsername = Login1.UserName.Trim();
        string stoocksPassword = Login1.Password.Trim();

        string passwordHash = stoocksPassword;
        //  Generate the Hashed Password if PasswordHashing Parameter from Web.Config is turned ON.        
        if (isDoPasswordHashing == 1)
        {
            passwordHash = dbOps.HashPassword(stoocksPassword);
        }
        bool isValid = IsValidCredentials(stoocksUsername, passwordHash);
        if (isValid)
        {
            CreateSession(stoocksUsername);
            Response.Redirect(links.HomePageLink);
        }
        else
        {
            Login1.FailureText = "Please Try Again.";
        }

    }

    private bool IsValidCredentials(string username, string password)
    {
        bool isValid = false;
        string queryString = @"SELECT Count(*) FROM stoocks.LOGIN WHERE Username='" + username + "' AND Password='" + password + "' ;";
        int retVal = dbOps.ExecuteScalar(queryString);

        if (retVal >= 1)
        {
            isValid = true;
        }
        return isValid;
    }

    private void CreateSession(string username)
    {
        //FormsAuthentication.Initialize();        
        ////The AddMinutes determines how long the user will be logged in after leaving the site if he doesn't log off.
        //FormsAuthenticationTicket fat = new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMinutes(30), false, username);
        //Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(fat)));
        //Response.Redirect(FormsAuthentication.GetRedirectUrl(username, false));

        //  Session.Timeout = 30;
        Session.Timeout = sessionTimeoutMinutes;
        Session.Add("username", username);

    }
    */
    #endregion Login Methods (Not In Use Since December 2007)


}
