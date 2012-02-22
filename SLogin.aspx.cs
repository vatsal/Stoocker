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
using System.Security.Cryptography;
using MySql.Data.MySqlClient;


public partial class SLogin : System.Web.UI.Page
{
    DBOperations dbOps;
    Links links;
    GUIVariables gui;
    Logger log;
    General general;
    ProcessingEngine engine;

    int sessionTimeoutMinutes = int.Parse(ConfigurationManager.AppSettings["sessionTimeoutMinutes"]);
    int isDoPasswordHashing = int.Parse(ConfigurationManager.AppSettings["isDoPasswordHashing"]);

    string commonIndices = ConfigurationManager.AppSettings["commonIndices"];
    //  string logPath = ConfigurationManager.AppSettings["logPath"];

    int maxRecommendedStocksTableRows = int.Parse(ConfigurationManager.AppSettings["maxRecommendedStocksTableRows"]);
    int minDiffBetweenHighPredictionsAndLowPredictions = int.Parse(ConfigurationManager.AppSettings["minDiffBetweenHighPredictionsAndLowPredictions"]);


    string dateFormatString = ConfigurationManager.AppSettings["dateFormatString"];

    string yesterday;
    string today;
    string tomorrow;
    string dayOfWeekTomorrow;   //  For use with StockRecommendationsTable.

    protected void Page_Load(object sender, EventArgs e)
    {        
        dbOps = (DBOperations)Application["dbOps"];
        links = (Links)Application["links"];
        gui = (GUIVariables)Application["gui"];
        log = (Logger)Application["log"];
        general = (General)Application["general"];
        engine = (ProcessingEngine)Application["engine"];
                
        #region CookieAlreadyExists
        //  START: If a stoockerCookie with the Username already exists, do not show the Login Page.
        string username = "";
        if (Request.Cookies["stoockerCookie"] != null)
        {
            HttpCookie stoockerCookie = Request.Cookies["stoockerCookie"];
            username = dbOps.Decrypt(stoockerCookie["username"].ToString().Trim());
        }
        if (!string.IsNullOrEmpty(username))
        {
            Response.Redirect(links.HomePageLink);
        }
        //  END: If a stoockerCookie with the Username already exists, do not show the Login Page.
        #endregion CookieAlreadyExists

        AboutStoockerLabel.Text = gui.GrayFontStart
            + gui.StoocksFont + " is a Stock Recommendation Engine." + gui.LineBreak
            + "Maintain a " + gui.GreenFontStart + "Portfolio" + gui.GreenFontEnd + " of your stocks." + gui.LineBreak
            + "Get updated " + gui.GreenFontStart + "News" + gui.GreenFontEnd + " about the latest happenings in the Stock Market." + gui.LineBreak
            + gui.GreenFontStart + "Predict" + gui.GreenFontEnd + " tomorrows movement of your favorite stocks." + gui.LineBreak
            + "See what the other users " + gui.GreenFontStart + "Recommend" + gui.GreenFontEnd + " about the stock movement for tomorrow."
            + gui.GrayFontEnd;

        yesterday = DateTime.Now.AddDays(-1).ToString(dateFormatString);
        today = DateTime.Now.ToString(dateFormatString);
        tomorrow = DateTime.Now.AddDays(+1).ToString(dateFormatString);
        dayOfWeekTomorrow = DateTime.Now.AddDays(+1).DayOfWeek.ToString();
        
        //  If Today=Friday Then Tomorrow=Monday. Why? Because Stock Markets are closed on Weekends.
        if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
            {
                tomorrow = DateTime.Now.AddDays(+3).ToString(dateFormatString);
                dayOfWeekTomorrow = DateTime.Now.AddDays(+3).DayOfWeek.ToString();
            }
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            {
                tomorrow = DateTime.Now.AddDays(+2).ToString(dateFormatString);
                dayOfWeekTomorrow = DateTime.Now.AddDays(+2).DayOfWeek.ToString();
            }
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                tomorrow = DateTime.Now.AddDays(+1).ToString(dateFormatString);
                dayOfWeekTomorrow = DateTime.Now.AddDays(+1).DayOfWeek.ToString();
            }
        }

        
        FillRecommendedStocksTable();
    }

    protected void LoginButton_Click(object sender, EventArgs e)
    {
        string message = "";
        //  string stoocksUsername = Login1.UserName.Trim();
        //  string stoocksPassword = Login1.Password.Trim();

        string stoocksUsername = UsernameTB.Text.Trim();
        string stoocksPassword = PasswordTB.Text.Trim();

        if (string.IsNullOrEmpty(stoocksUsername) || string.IsNullOrEmpty(stoocksPassword))
        {
            message = gui.RedFontStart + "Please Enter Your Username/Password" + gui.RedFontEnd;
        }
        else if(!general.IsAlphabetOrNumber(stoocksUsername))
        {
            message = gui.RedFontStart + "Username can only contain Alphabets & Numbers." + gui.RedFontEnd;
        }
        else if(!general.IsAlphabetOrNumber(stoocksPassword))
        {
            message = gui.RedFontStart + "Password can only contain Alphabets & Numbers." + gui.RedFontEnd;
        }
        else
        {
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
                log.Log(stoocksUsername + " Logged.");                                
                Response.Redirect(links.HomePageLink);            
            }
            else
            {
                message = gui.RedFontStart + "Please Try Again" + gui.RedFontEnd;                            
            }
        }
        MessageLabel.Text = message;
    }

    

    private bool IsValidCredentials(string username, string password)
    {
        bool isValid = false;        
        string queryString = @"SELECT Count(*) FROM stoocks.login WHERE Username='" + username + "' AND Password='" + password + "' ;";        
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
        
        //  Session.Timeout = sessionTimeoutMinutes;
        //  Session.Add("username", username);




        HttpCookie stoockerCookie = new HttpCookie("stoockerCookie");
        stoockerCookie["username"] = dbOps.Encrypt(username);        
        //  stoockerCookie.Expires = DateTime.Now.AddHours(1);

        if (username == ConfigurationManager.AppSettings["DemoUser"])
        {
            stoockerCookie.Expires = DateTime.Now.AddMinutes(30);
        }

        Response.Cookies.Add(stoockerCookie);
        
    }



    protected void NewsLink_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.NewsPageLink);
    }

    private void FillRecommendedStocksTable()
    {        
        //SELECT StockSymbol, (HighVotes/(HighVotes+LowVotes) * 100) FROM stockprediction where Date='2008-02-04'
        //AND PredictedMovement = 1
        //AND (HighVotes - LowVotes) > 1
        //AND SumHighVotesMulWeight >= SumLowVotesMulWeight
        //ORDER BY HighVotes desc limit 5;
                
        //  From Web.Config //  maxRecommendedStocksTableRows        //  minPredictionsPerStock

        try
        {
            //  string queryString = "SELECT StockSymbol, (HighVotes/(HighVotes+LowVotes) * 100) FROM stockprediction where Date='" + tomorrow + "' AND PredictedMovement = 1 AND (HighVotes - LowVotes) > " + minDiffBetweenHighPredictionsAndLowPredictions + " AND SumHighVotesMulWeight >= SumLowVotesMulWeight ORDER BY HighVotes desc limit " + maxRecommendedStocksTableRows;
            string queryString = "SELECT StockSymbol, HighVotes, (HighVotes+LowVotes) FROM stockprediction where Date='" + tomorrow + "' AND PredictedMovement = 1 AND (HighVotes - LowVotes) > " + minDiffBetweenHighPredictionsAndLowPredictions + " AND SumHighVotesMulWeight >= SumLowVotesMulWeight ORDER BY HighVotes desc limit " + maxRecommendedStocksTableRows;
            MySqlDataReader retList = dbOps.ExecuteReader(queryString);

            if (retList != null && retList.HasRows)
            {
                RecommendedStocksLabel.Text = "Highest Recommended Gainers for " + dayOfWeekTomorrow + ", " + tomorrow;

                TableRow row; 
                TableCell symbolCell; 
                TableCell percentHighCell;

                //row = new TableRow();
                //symbolCell = new TableCell();
                //percentHighCell = new TableCell();

                //symbolCell.Text = gui.RedFontStart + "Stock" + gui.RedFontEnd;
                //percentHighCell.Text = gui.RedFontStart + "(Users who Recommend this Stock will go UP / Total Number of Users who made a Prediction on this Stock)" + gui.RedFontEnd;

                //row.Cells.Add(symbolCell);
                //row.Cells.Add(percentHighCell);

                //RecommendedStocksTable.Rows.Add(row);

                while (retList.Read())
                {
                    row = new TableRow();
                    symbolCell = new TableCell();
                    percentHighCell = new TableCell();

                    string symbol = retList.GetString(0);
                    double highVotes = retList.GetDouble(1);
                    double totalVotes = retList.GetDouble(2);

                    symbolCell.Text = gui.BlueFontStart + symbol + gui.BlueFontEnd;
                    percentHighCell.Text = gui.GrayFontStart 
                        + gui.GreenFontStart + "[" + highVotes + " out of " + totalVotes.ToString() + "]" + gui.GreenFontEnd
                        + " Experts Recommend " + gui.GreenFontStart + "Upward" + gui.GreenFontEnd + " Movement" 
                        + gui.GrayFontEnd;

                    row.Cells.Add(symbolCell);
                    row.Cells.Add(percentHighCell);

                    RecommendedStocksTable.Rows.Add(row);
                }

                CollaborativeRecommendationsLabel.Text = gui.GrayFontStart + " (Based on Collaborative Recommendations from Users.)" + gui.GrayFontEnd;

            }
        }
        catch (Exception ex)
        {
            
        }


    }


    protected void DemoLink_Click(object sender, EventArgs e)
    {
        string message = "";

        string stoocksUsername = ConfigurationManager.AppSettings["DemoUser"].Trim();
        string stoocksPassword = ConfigurationManager.AppSettings["DemoPass"].Trim();
        
        if (string.IsNullOrEmpty(stoocksUsername) || string.IsNullOrEmpty(stoocksPassword))
        {
            message = gui.RedFontStart + "Please Enter Your Username/Password" + gui.RedFontEnd;
        }
        else if (!general.IsAlphabetOrNumber(stoocksUsername))
        {
            message = gui.RedFontStart + "Username can only contain Alphabets & Numbers." + gui.RedFontEnd;
        }
        else if (!general.IsAlphabetOrNumber(stoocksPassword))
        {
            message = gui.RedFontStart + "Password can only contain Alphabets & Numbers." + gui.RedFontEnd;
        }
        else
        {
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
                log.Log(stoocksUsername + " Logged.");
                Response.Redirect(links.HomePageLink);
            }
            else
            {
                message = gui.RedFontStart + "Please Try Again" + gui.RedFontEnd;
            }
        }
        MessageLabel.Text = message;
    }
}
