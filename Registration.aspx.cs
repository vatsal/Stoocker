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

using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public partial class Registration : System.Web.UI.Page
{
    DBOperations dbOps;
    Links links;
    General general;
    GUIVariables gui;
    ProcessingEngine engine;

    //  static Label ErrorLabel;
    //  static DropDownList ExpertiseDDL;
       
    int sessionTimeoutMinutes = int.Parse(ConfigurationManager.AppSettings["sessionTimeoutMinutes"]);
    double initialUserWeight = double.Parse(ConfigurationManager.AppSettings["initialUserWeight"]);
    int isDoPasswordHashing = int.Parse(ConfigurationManager.AppSettings["isDoPasswordHashing"]);
    
    protected void Page_Load(object sender, EventArgs e)
    {
        dbOps = (DBOperations)Application["dbOps"];
        links = (Links)Application["links"];
        general = (General)Application["general"];
        gui = (GUIVariables)Application["gui"];
        engine = (ProcessingEngine)Application["engine"];

        AboutStoockerLabel.Text = gui.GrayFontStart 
            + gui.StoocksFont + " is a Stock Recommendation Engine." + gui.LineBreak
            + "Maintain a " + gui.GreenFontStart + "Portfolio" + gui.GreenFontEnd + " of your stocks." + gui.LineBreak
            + "Get updated " + gui.GreenFontStart + "News" + gui.GreenFontEnd + " about the latest happenings in the Stock Market." + gui.LineBreak
            + gui.GreenFontStart + "Predict" + gui.GreenFontEnd + " tomorrows movement of your favorite stocks." + gui.LineBreak
            + "See what the other users " + gui.GreenFontStart + "Recommend" + gui.GreenFontEnd + " about the stock movement for tomorrow."
            + gui.GrayFontEnd;

    }

    protected void RegistrationButton_Click(object sender, EventArgs e)
    {
        //  ErrorLabel = (Label)CreateUserWizard1.CreateUserStep.ContentTemplateContainer.FindControl("ErrorLabel");
        //  ExpertiseDDL = (DropDownList)CreateUserWizard1.CreateUserStep.ContentTemplateContainer.FindControl("ExpertiseDDL");                    

        string username = UserName.Text.Trim();
        string password = Password.Text.Trim();
        string confirmPassword = ConfirmPassword.Text.Trim();
        string email = Email.Text.Trim();


        int expertiseLevel = 2; //  By Default, all Predictors are Intermediates.
        //  bool isExpertiseParsable = int.TryParse(ExpertiseDDL.SelectedValue, out expertiseLevel);

        bool isValid = false;
        string errorMessage = "";
        int userRegistrationCode = 0;

        if (username != "" && password != "" && confirmPassword != "" && email != "")
        {
            if (username.Length < 4)
            {
                errorMessage = "Username should be longer than 4 characters";
            }
            else if (password.Length < 4)
            {
                errorMessage = "Password should be longer than 4 characters";
            }
            else if (!general.IsAlphabetOrNumber(username))
            {
                errorMessage = gui.RedFontStart + "Username can only contain Alphabets & Numbers." + gui.RedFontEnd;
            }
            else if (!general.IsAlphabetOrNumber(password))
            {
                errorMessage = gui.RedFontStart + "Password can only contain Alphabets & Numbers." + gui.RedFontEnd;
            }
            else if ((email.Length < 4) || (!general.IsValidEMail(email)))
            {
                errorMessage = "Enter a valid E-Mail Address";
            }
            
            else
            {
                if (password == confirmPassword)
                {
                    //  Generate the Hashed Password.

                    string passwordHash = password;
                    if (isDoPasswordHashing == 1)
                    {
                        passwordHash = dbOps.HashPassword(password);
                    }
                    //  userRegistrationCode = CreateUser(username, password, email, expertiseLevel);
                    userRegistrationCode = CreateUser(username, passwordHash, email, expertiseLevel);

                    if (userRegistrationCode == 0)
                        isValid = true;
                    else if (userRegistrationCode == 1)
                    {
                        errorMessage = "User Already Exists. Please Try a different Username";
                    }
                    else
                    {
                        errorMessage = "Please Try Again";
                    }
                }
            }
        }
        else
        {
            isValid = false;
        }

        if (errorMessage != "")
        {
            ErrorLabel.Text = errorMessage;
        }

        if (isValid)
        {
            CreateSession(username);
            //  Response.Redirect(homePageLink);
            Response.Redirect(links.InterestsPageLink);
        }
        else
        {

        }
    }


    /// <summary>
    /// Create the User if not already existing inside Stoocks
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="email"></param>
    /// <param name="expertiseLevel"></param>
    /// <returns>(0) -> Success, (1)-> User already exists, (-1) -> Error</returns>
    private int CreateUser(string username, string password, string email, int expertiseLevel)
    {        
        //  DBOperations dbOps = new DBOperations();
        string queryString = @"SELECT Count(*) FROM stoocks.registration WHERE username='" + username + "';";
        int retVal = dbOps.ExecuteScalar(queryString);

        if (retVal == 0)    //  No User with the Username=username exists. Create the User.
        {
            queryString = @"INSERT INTO stoocks.registration VALUES ('" + username + "', '" + password + "', '" + email + "', '" + expertiseLevel + "');";
            retVal = dbOps.ExecuteNonQuery(queryString);
            if (retVal >= 0)    //  User successfully registered.
            {
                retVal = 0;
                //  Add the User to the Login DB.
                queryString = @"INSERT INTO stoocks.login VALUES ('" + username + "', '" + password + "' );";
                retVal = dbOps.ExecuteNonQuery(queryString);
                if (retVal >= 0)
                {
                    retVal = 0;
                    //  double currentWeight = 1.0;
                    double currentWeight = initialUserWeight;
                    queryString = @"INSERT INTO stoocks.expertise VALUES ('" + username + "', '" + expertiseLevel + "', '" + currentWeight + "' );";
                    retVal = dbOps.ExecuteNonQuery(queryString);
                    if (retVal >= 0)
                    {
                        retVal = 0;
                    }
                    else
                    {
                        retVal = -1;
                    }
                }
                else
                {
                    retVal = -1;
                }
            }
            else
            {
                retVal = -1;
            }
        }
        else if (retVal > 0)    //  User Already Exists.
        {
            retVal = 1;    
        }
        else    //  An error occurred.
        {
            retVal = -1;
        }
        return retVal;
    }

    

    private void CreateSession(string username)
    {
        //FormsAuthentication.Initialize();        
        ////The AddMinutes determines how long the user will be logged in after leaving the site if he doesn't log off.
        //FormsAuthenticationTicket fat = new FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMinutes(30), false, username);
        //Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(fat)));
        //Response.Redirect(FormsAuthentication.GetRedirectUrl(username, false));

        //  Session.Timeout = 30;
        //  Session.Timeout = sessionTimeoutMinutes;
        //  Session.Add("username", username);

        HttpCookie stoockerCookie = new HttpCookie("stoockerCookie");
        stoockerCookie["username"] = dbOps.Encrypt(username);
        //  stoockerCookie.Expires = DateTime.Now.AddHours(1);
        Response.Cookies.Add(stoockerCookie);
    }



    
}
