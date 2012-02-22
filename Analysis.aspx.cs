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
using MySql.Data.MySqlClient;

public partial class Analysis : System.Web.UI.Page
{
    DBOperations dbOps;
    GUIVariables gui;
    RssReader rssReader;
    Links links;
    ProcessingEngine engine;

    string username;
    string symbol;    

    string dateFormatString = ConfigurationManager.AppSettings["dateFormatString"];

    string yesterday;
    string today;
    string tomorrow;

    int upPredict;
    int downPredict; 


    protected void Page_Load(object sender, EventArgs e)
    {         
        dbOps = (DBOperations)Application["dbOps"];
        rssReader = (RssReader)Application["rssReader"];
        gui = (GUIVariables)Application["gui"];
        links = (Links)Application["links"];
        engine = (ProcessingEngine)Application["engine"];

        //  username = Convert.ToString(Session["username"]);

        if (Request.Cookies["stoockerCookie"] != null)
        {
            HttpCookie stoockerCookie = Request.Cookies["stoockerCookie"];
            username = dbOps.Decrypt(stoockerCookie["username"].ToString().Trim());
        }

        if (string.IsNullOrEmpty(username))
        {
            Response.Redirect(links.SessionExpiredPageLink);
        }
        else
        {
            upPredict = (int)ProcessingEngine.Movement.Up;
            downPredict = (int)ProcessingEngine.Movement.Down;

            yesterday = DateTime.Now.AddDays(-1).ToString(dateFormatString);
            today = DateTime.Now.ToString(dateFormatString);
            tomorrow = DateTime.Now.AddDays(+1).ToString(dateFormatString);

            //  If Today=Friday Then Tomorrow=Monday. Why? Because Stock Markets are closed on Weekends.
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                    tomorrow = DateTime.Now.AddDays(+3).ToString(dateFormatString);
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    tomorrow = DateTime.Now.AddDays(+2).ToString(dateFormatString);
                if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    tomorrow = DateTime.Now.AddDays(+1).ToString(dateFormatString);
            }
             

            symbol = Convert.ToString(Request.QueryString["stock"]);
            if (string.IsNullOrEmpty(symbol))
            {
                Response.Redirect(links.HomePageLink);
            }
            else
            {                
                StockDetailsLabel.Text = LoadStockDetails(symbol);
                LoadYahooFinanceBadgeIFrame(symbol);

                string newsFeeds = rssReader.LoadPerStockNews(symbol);
                if (!string.IsNullOrEmpty(newsFeeds))
                {
                    NewsMessageLabel.Text = gui.BoldFontStart + "Latest News & Recommended Readings from " + gui.StoocksFont + gui.BoldFontEnd;
                    NewsLabel.Text = newsFeeds;
                }
            }
        }
        
    }
    

    private string LoadStockDetails(string symbol)
    {
        string stockDetails = "";

        //  Populate the UserPrediction and StockPrediction DB.
        //  UserPrediction(Username,PredictedMovement,Symbol,Time,Date)
        //  StockPrediction(StockSymbol,Date,HighVotes,LowVotes,SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement,ActualMovement)

        stockDetails = gui.BoldFontStart
            + gui.GreenFontStart + "Symbol: " + gui.GreenFontEnd + gui.BlueFontStart + symbol + gui.BlueFontEnd + gui.LineBreak
            + gui.BoldFontEnd;

        //  Fetch HighVotes | LowVotes | Current Price | Closing Price
        string queryString = "SELECT HighVotes, LowVotes, PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol = '" + symbol + "' AND Date='" + tomorrow + "';";
        
        //  ArrayList retList = dbOps.QueryMultipleInformationSelect(queryString);
        MySqlDataReader retList = dbOps.ExecuteReader(queryString);
        
        //  if (retList.Count > 0)
        if (retList != null && retList.HasRows)
        {
            while (retList.Read())
            {
                //string highVotes = Convert.ToString(retList[0]);
                //string lowVotes = Convert.ToString(retList[1]);
                //string predictedMovement = int.Parse(Convert.ToString(retList[2])) == 1 ? "Up" : "Down";

                string highVotes = retList.GetString(0);
                string lowVotes = retList.GetString(1);
                
                //  string predictedMovement = retList.GetInt32(2) == 1 ? "Up" : "Down";
                string predictedMovement = retList.GetInt32(2) == (int)ProcessingEngine.Movement.Up ? "Up" : "Down";


                int totalVotes = int.Parse(highVotes) + int.Parse(lowVotes);

                stockDetails = stockDetails + gui.BoldFontStart
                    + gui.GreenFontStart + "Total Prediction Count:" + gui.BlueFontStart + totalVotes + gui.BlueFontEnd + gui.GreenFontEnd + gui.LineBreak
                    + gui.GreenFontStart + "Number of Upwards Prediction: " + gui.GreenFontEnd + gui.BlueFontStart + highVotes + gui.BlueFontEnd + gui.LineBreak
                    + gui.GreenFontStart + "Number of Downwards Prediction: " + gui.GreenFontEnd + gui.BlueFontStart + lowVotes + gui.BlueFontEnd + gui.LineBreak
                    + gui.LineBreak
                    + gui.GreenFontStart + "Predicted Movement: " + gui.GreenFontEnd + predictedMovement + gui.LineBreak
                    + gui.BoldFontEnd;
            }
            retList.Close();
        }
        else
        {
            stockDetails = stockDetails + gui.BoldFontStart
              + gui.GreenFontStart + "No Stoocker has made a prediction for " + gui.BlueFontStart + symbol + gui.BlueFontEnd + ". Kick Things off!!" + gui.GreenFontEnd + gui.LineBreak
              + gui.BoldFontEnd;
        }

        

        //  Find the Overall Performance of all the Stoockers on this Stock.
        //  This gives an overall picture of how Stoocks has performed over the entire history of the Stock.
        //  Overall Performance = (Predictions where Actual=Predicted)/Total Number of Predictions.
        
        return stockDetails;
    }


    private void LoadYahooFinanceBadgeIFrame(string alreadySelectedStocks)
    {
        YahooFinanceBadge yfb = new YahooFinanceBadge();
        if (yfb.IsShowYFB)
        {            
            YahooFinanceBadgeIFramePlaceHolder.Controls.Add(yfb.CreateYBF(alreadySelectedStocks, 2, true, 0, "no", 200, 400));
        }
    }

    #region Old_Code_Before_16Jan2008
    //protected void UpPredictionButton_Click(object sender, ImageClickEventArgs e)
    //{        
    //    string queryString = "";
    //    int retVal = 0;

    //    //  ArrayList retList;
    //    MySqlDataReader retList;

    //    //  int expertise = 2;  //  Default Expertise.
    //    //  double userWeight = 100;   //  Default CurrentWeight.

    //    int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
    //    double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.


    //    //  Select Expertise and Weight from Expertise DB
    //    //  Expertise(Username,Expertise,CurrentWeight)
    //    queryString = @"SELECT Expertise,CurrentWeight FROM stoocks.expertise WHERE Username='" + username + "' ;";

    //    //  retList = dbOps.QueryMultipleInformationSelect(queryString);
    //    retList = dbOps.ExecuteReader(queryString);

    //    if (retList != null && retList.HasRows)
    //        while (retList.Read())
    //        {
    //            //  expertise = Convert.ToInt32(retList[0]);
    //            //  currentWeight = Convert.ToDouble(retList[1]);

    //            expertise = retList.GetInt32(0);
    //            userWeight = retList.GetDouble(1);
    //        }
    //    retList.Close();

    //    //  int perStockExpertise = 2;
    //    //  double perStockWeight = 100;

    //    int perStockExpertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;
    //    double perStockWeight = engine.InitialPerStockUserWeight;

    //    queryString = @"SELECT Expertise, CurrentWeight FROM stoocks.perstockexpertise WHERE Username='" + username + "' AND Symbol='" + symbol + "';";

    //    //  retList = dbOps.QueryMultipleInformationSelect(queryString);
    //    retList = dbOps.ExecuteReader(queryString);

    //    if (retList != null && retList.HasRows)
    //        while (retList.Read())
    //        {
    //            //  perStockExpertise = Convert.ToInt32(retList[0]);
    //            //  perStockWeight = Convert.ToDouble(retList[1]);

    //            perStockExpertise = retList.GetInt32(0);
    //            perStockWeight = retList.GetDouble(1);
    //        }
    //    retList.Close();

    //    //  double currentUserWeighting = (double)(expertise * currentWeight * upPredict);        
    //    double currentUserWeighting = (double)(expertise * userWeight * perStockExpertise * perStockWeight * upPredict);

    //    double sumHighVotesMulWeight = 0;
    //    double sumLowVotesMulWeight = 0;
    //    int predictedMovement;

    //    //  queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM Stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";
    //    queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + tomorrow + "';";

    //    //  retList = dbOps.QueryMultipleInformationSelect(queryString);
    //    retList = dbOps.ExecuteReader(queryString);

    //    if (retList != null && retList.HasRows)
    //        while (retList.Read())
    //        {
    //            //  sumHighVotesMulWeight = Convert.ToDouble(retList[0]);
    //            //  sumLowVotesMulWeight = Convert.ToDouble(retList[1]);
    //            //  predictedMovement = Convert.ToInt32(retList[2]);

    //            sumHighVotesMulWeight = retList.GetDouble(0);
    //            sumLowVotesMulWeight = retList.GetDouble(1);
    //            predictedMovement = retList.GetInt32(2);
    //        }
    //    retList.Close();

    //    sumHighVotesMulWeight = sumHighVotesMulWeight + currentUserWeighting;

    //    if (sumHighVotesMulWeight > Math.Abs(sumLowVotesMulWeight))
    //    {
    //        //  predictedMovement = 1;
    //        predictedMovement = (int)ProcessingEngine.Movement.Up;
    //    }
    //    else
    //    {
    //        predictedMovement = (int)ProcessingEngine.Movement.Down;
    //    }

    //    //  Populate the UserPrediction and StockPrediction DB.
    //    //  UserPrediction(Username,PredictedMovement,Symbol,Time,Date)
    //    //  StockPrediction(StockSymbol,Date,HighVotes,LowVotes,SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement,ActualMovement)

    //    //  C# Code to Get Date in MySQL Format: System.DateTime.Now.ToString(dateFormatString);
    //    //  INSERT INTO StockPrediction VALUES ('GOOG','2007-07-31',1,0,0,0,1,1) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1;
    //    //  string queryString = @"UPDATE Stoocks.StockPrediction SET HighVotes = HighVotes + 1 WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";

    //    //  queryString = "INSERT INTO Stoocks.UserPrediction VALUES ('" + username + "'," + upPredict + ",'" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + DateTime.Now.ToString(dateFormatString) + "');";
    //    queryString = "INSERT INTO stoocks.userprediction VALUES ('" + username + "'," + upPredict + ", 0, '" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + tomorrow + "');";
    //    retVal = dbOps.ExecuteNonQuery(queryString);

    //    //  queryString = "INSERT INTO Stoocks.StockPrediction VALUES ('" + symbol + "','" + DateTime.Now.ToString(dateFormatString) + "',1,0," + sumHighVotesMulWeight + ",0,1,1) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1, SumHighVotesMulWeight=" + sumHighVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
    //    queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',1,0," + sumHighVotesMulWeight + ",0,1,0) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1, SumHighVotesMulWeight=" + sumHighVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
    //    retVal = dbOps.ExecuteNonQuery(queryString);
        
    //    UpPredictionButton.Enabled = false;
    //    UpPredictionButton.ImageUrl = @"~/Images/upgraymod.png";

    //    MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
    //        + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;
 
    //}


    //protected void DownPredictionButton_Click(object sender, ImageClickEventArgs e)
    //{        
    //    string queryString = "";
    //    int retVal = 0;

    //    //  ArrayList retList;
    //    MySqlDataReader retList;

    //    //  int expertise = 2;  //  Default Expertise.
    //    //  double userWeight = 100;   //  Default CurrentWeight.

    //    int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
    //    double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.


    //    //  Select Expertise and Weight from Expertise DB
    //    //  Expertise(Username,Expertise,CurrentWeight)
    //    queryString = @"SELECT Expertise,CurrentWeight FROM stoocks.expertise WHERE Username='" + username + "' ;";

    //    //  retList = dbOps.QueryMultipleInformationSelect(queryString);
    //    retList = dbOps.ExecuteReader(queryString);

    //    if (retList != null && retList.HasRows)
    //        while (retList.Read())
    //        {
    //            //  expertise = Convert.ToInt32(retList[0]);
    //            //  currentWeight = Convert.ToDouble(retList[1]);

    //            expertise = retList.GetInt32(0);
    //            userWeight = retList.GetDouble(1);
    //        }
    //    retList.Close();

    //    //  int perStockExpertise = 2;
    //    //  double perStockWeight = 100;

    //    int perStockExpertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;
    //    double perStockWeight = engine.InitialPerStockUserWeight;

    //    queryString = @"SELECT Expertise, CurrentWeight FROM stoocks.perstockexpertise WHERE Username='" + username + "' AND Symbol='" + symbol + "';";

    //    //  retList = dbOps.QueryMultipleInformationSelect(queryString);
    //    retList = dbOps.ExecuteReader(queryString);

    //    if (retList != null && retList.HasRows)
    //        while (retList.Read())
    //        {
    //            //  perStockExpertise = Convert.ToInt32(retList[0]);
    //            //  perStockWeight = Convert.ToDouble(retList[1]);

    //            perStockExpertise = retList.GetInt32(0);
    //            perStockWeight = retList.GetDouble(1);
    //        }
    //    retList.Close();

    //    //  double currentUserWeighting = (double)(expertise * currentWeight * downPredict);
    //    double currentUserWeighting = (double)(expertise * userWeight * perStockExpertise * perStockWeight * downPredict);

    //    double sumHighVotesMulWeight = 0;
    //    double sumLowVotesMulWeight = 0;
    //    int predictedMovement;

    //    //  queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM Stoocks.StockPrediction WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";
    //    queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + tomorrow + "';";

    //    //  retList = dbOps.QueryMultipleInformationSelect(queryString);
    //    retList = dbOps.ExecuteReader(queryString);

    //    if (retList != null && retList.HasRows)
    //        while (retList.Read())
    //        {
    //            //  sumHighVotesMulWeight = Convert.ToDouble(retList[0]);
    //            //  sumLowVotesMulWeight = Convert.ToDouble(retList[1]);
    //            //  predictedMovement = Convert.ToInt32(retList[2]);

    //            sumHighVotesMulWeight = retList.GetDouble(0);
    //            sumLowVotesMulWeight = retList.GetDouble(1);
    //            predictedMovement = retList.GetInt32(2);
    //        }
    //    retList.Close();

    //    sumLowVotesMulWeight = sumLowVotesMulWeight + currentUserWeighting;

    //    if (sumHighVotesMulWeight > Math.Abs(sumLowVotesMulWeight))
    //    {
    //        //  predictedMovement = 1;
    //        predictedMovement = (int)ProcessingEngine.Movement.Up;
    //    }
    //    else
    //    {
    //        //  predictedMovement = -1;
    //        predictedMovement = (int)ProcessingEngine.Movement.Down;
    //    }

    //    //  Populate the UserPrediction and StockPrediction DB.
    //    //  UserPrediction(Username,PredictedMovement,Symbol,Time,Date)
    //    //  StockPrediction(StockSymbol,Date,HighVotes,LowVotes,SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement,ActualMovement)

    //    //  C# Code to Get Date in MySQL Format: System.DateTime.Now.ToString(dateFormatString);
    //    //  INSERT INTO StockPrediction VALUES ('GOOG','2007-07-31',0,1,0,0,1,1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1;
    //    //  string queryString = @"UPDATE Stoocks.StockPrediction SET LowVotes = LowVotes + 1 WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";

    //    //  queryString = "INSERT INTO Stoocks.UserPrediction VALUES ('" + username + "'," + downPredict + ",'" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + DateTime.Now.ToString(dateFormatString) + "');";
    //    queryString = "INSERT INTO stoocks.userprediction VALUES ('" + username + "'," + downPredict + ", 0, '" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + tomorrow + "');";
    //    retVal = dbOps.ExecuteNonQuery(queryString);

    //    //  queryString = "INSERT INTO Stoocks.StockPrediction VALUES ('" + symbol + "','" + DateTime.Now.ToString(dateFormatString) + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
    //    queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
    //    retVal = dbOps.ExecuteNonQuery(queryString);
        
    //    DownPredictionButton.Enabled = false;
    //    DownPredictionButton.ImageUrl = @"~/Images/downgraymod.png";

    //    MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
    //        + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

    //}
    #endregion Old_Code_Before_16Jan2008



    protected void UpPredictionButton_Click(object sender, EventArgs e)
    {
        string queryString = "";
        int retVal = 0;

        MySqlDataReader retList;

        //  int expertise = 2;  //  Default Expertise.
        //  double currentWeight = 100;   //  Default CurrentWeight.

        int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
        double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.


        //  Select Expertise and Weight from Expertise(Username,Expertise,CurrentWeight)
        queryString = @"SELECT Expertise,CurrentWeight FROM stoocks.expertise WHERE Username='" + username + "' ;";

        //  retList = dbOps.QueryMultipleInformationSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
            while (retList.Read())
            {
                expertise = retList.GetInt32(0);
                userWeight = retList.GetDouble(1);
            }
        retList.Close();

        //  int perStockExpertise = 2;
        //  double perStockWeight = 100;

        int perStockExpertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;
        double perStockWeight = engine.InitialPerStockUserWeight;

        queryString = @"SELECT Expertise, CurrentWeight FROM stoocks.perstockexpertise WHERE Username='" + username + "' AND Symbol='" + symbol + "';";

        //  retList = dbOps.QueryMultipleInformationSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
            while (retList.Read())
            {
                perStockExpertise = retList.GetInt32(0);
                perStockWeight = retList.GetDouble(1);
            }
        retList.Close();

        //  If user is just an Intermediate Stoocker, do not count his [Expertise * Weight * Prediction] in the final updation for stockprediction table.
        if (expertise == (int)ProcessingEngine.ExpertiseLevel.Intermediate || perStockExpertise == (int)ProcessingEngine.ExpertiseLevel.Intermediate)
        {
            double sumHighVotesMulWeight = 0;
            double sumLowVotesMulWeight = 0;
            int predictedMovement = 0;

            //  queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM Stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";
            queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + tomorrow + "';";
            retList = dbOps.ExecuteReader(queryString);

            if (retList != null && retList.HasRows)
                while (retList.Read())
                {
                    sumHighVotesMulWeight = retList.GetDouble(0);
                    sumLowVotesMulWeight = retList.GetDouble(1);
                    predictedMovement = retList.GetInt32(2);
                }
            retList.Close();


            queryString = "INSERT INTO stoocks.userprediction VALUES ('" + username + "'," + upPredict + ", 0, '" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + tomorrow + "');";
            retVal = dbOps.ExecuteNonQuery(queryString);

            queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',1,0," + sumHighVotesMulWeight + ",0,1,0) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1, SumHighVotesMulWeight=" + sumHighVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            retVal = dbOps.ExecuteNonQuery(queryString);

        }
        else
        {

            //  If the User is a consistent Loser,(Expertise & PerStockExpertise both are of Beginner Level, Invert his Prediction)
            if (expertise == (int)ProcessingEngine.ExpertiseLevel.Beginner && perStockExpertise == (int)ProcessingEngine.ExpertiseLevel.Beginner)
            {
                upPredict = (int)ProcessingEngine.Movement.Down;
            }

            //  double currentUserWeighting = (double)(expertise * currentWeight * upPredict);        
            double currentUserWeighting = (double)(expertise * userWeight * perStockExpertise * perStockWeight * upPredict);

            double sumHighVotesMulWeight = 0;
            double sumLowVotesMulWeight = 0;
            int predictedMovement;

            //  queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM Stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";
            queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + tomorrow + "';";

            //  retList = dbOps.QueryMultipleInformationSelect(queryString);
            retList = dbOps.ExecuteReader(queryString);

            if (retList != null && retList.HasRows)
                while (retList.Read())
                {
                    sumHighVotesMulWeight = retList.GetDouble(0);
                    sumLowVotesMulWeight = retList.GetDouble(1);
                    predictedMovement = retList.GetInt32(2);
                }
            retList.Close();

            sumHighVotesMulWeight = sumHighVotesMulWeight + currentUserWeighting;

            if (sumHighVotesMulWeight > Math.Abs(sumLowVotesMulWeight))
            {
                predictedMovement = (int)ProcessingEngine.Movement.Up;
            }
            else
            {
                predictedMovement = (int)ProcessingEngine.Movement.Down;
            }

            //  Populate the UserPrediction and StockPrediction DB.
            //  UserPrediction(Username,PredictedMovement,Symbol,Time,Date)
            //  StockPrediction(StockSymbol,Date,HighVotes,LowVotes,SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement,ActualMovement)

            //  C# Code to Get Date in MySQL Format: System.DateTime.Now.ToString(dateFormatString);
            //  INSERT INTO StockPrediction VALUES ('GOOG','2007-07-31',1,0,0,0,1,1) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1;
            //  string queryString = @"UPDATE Stoocks.StockPrediction SET HighVotes = HighVotes + 1 WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";

            //  queryString = "INSERT INTO Stoocks.UserPrediction VALUES ('" + username + "'," + upPredict + ",'" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + DateTime.Now.ToString(dateFormatString) + "');";
            queryString = "INSERT INTO stoocks.userprediction VALUES ('" + username + "'," + upPredict + ", 0, '" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + tomorrow + "');";
            retVal = dbOps.ExecuteNonQuery(queryString);

            //  queryString = "INSERT INTO Stoocks.StockPrediction VALUES ('" + symbol + "','" + DateTime.Now.ToString(dateFormatString) + "',1,0," + sumHighVotesMulWeight + ",0,1,1) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1, SumHighVotesMulWeight=" + sumHighVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',1,0," + sumHighVotesMulWeight + ",0,1,0) ON DUPLICATE KEY UPDATE HighVotes=HighVotes+1, SumHighVotesMulWeight=" + sumHighVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            retVal = dbOps.ExecuteNonQuery(queryString);
                        
        }

        UpPredictionButton.Enabled = false;
        UpPredictionButton.ImageUrl = @"~/Images/upgraymod.png";

        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

        //  Response.Redirect("~/Analysis.aspx?stock=" + symbol);
        StockDetailsLabel.Text = LoadStockDetails(symbol);

    }

    protected void DownPredictionButton_Click(object sender, EventArgs e)
    {        
        string queryString = "";
        int retVal = 0;

        MySqlDataReader retList;

        //  int expertise = 2;  //  Default Expertise.
        //  double currentWeight = 100;   //  Default CurrentWeight.

        int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
        double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.

        //  Select Expertise and Weight from Expertise(Username,Expertise,CurrentWeight)
        queryString = @"SELECT Expertise,CurrentWeight FROM stoocks.expertise WHERE Username='" + username + "' ;";

        //  retList = dbOps.QueryMultipleInformationSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
            while (retList.Read())
            {
                expertise = retList.GetInt32(0);
                userWeight = retList.GetDouble(1);
            }
        retList.Close();

        //  int perStockExpertise = 2;
        //  double perStockWeight = 100;

        int perStockExpertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;
        double perStockWeight = engine.InitialPerStockUserWeight;

        queryString = @"SELECT Expertise, CurrentWeight FROM stoocks.perstockexpertise WHERE Username='" + username + "' AND Symbol='" + symbol + "';";

        //  retList = dbOps.QueryMultipleInformationSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
            while (retList.Read())
            {
                perStockExpertise = retList.GetInt32(0);
                perStockWeight = retList.GetDouble(1);
            }
        retList.Close();

        //  If user is just an Intermediate Stoocker, do not count his [Expertise * Weight * Prediction] in the final updation for stockprediction table.
        if (expertise == (int)ProcessingEngine.ExpertiseLevel.Intermediate || perStockExpertise == (int)ProcessingEngine.ExpertiseLevel.Intermediate)
        {
            double sumHighVotesMulWeight = 0;
            double sumLowVotesMulWeight = 0;
            int predictedMovement = 0;

            //  queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM Stoocks.StockPrediction WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";
            queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + tomorrow + "';";

            //  retList = dbOps.QueryMultipleInformationSelect(queryString);
            retList = dbOps.ExecuteReader(queryString);

            if (retList != null && retList.HasRows)
                while (retList.Read())
                {
                    sumHighVotesMulWeight = retList.GetDouble(0);
                    sumLowVotesMulWeight = retList.GetDouble(1);
                    predictedMovement = retList.GetInt32(2);
                }
            retList.Close();

            queryString = "INSERT INTO stoocks.userprediction VALUES ('" + username + "'," + downPredict + ", 0, '" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + tomorrow + "');";
            retVal = dbOps.ExecuteNonQuery(queryString);

            queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            retVal = dbOps.ExecuteNonQuery(queryString);

        }
        else
        {

            //  If the User is a consistent Loser,(Expertise & PerStockExpertise both are of Beginner Level, Invert his Prediction)
            if (expertise == (int)ProcessingEngine.ExpertiseLevel.Beginner && perStockExpertise == (int)ProcessingEngine.ExpertiseLevel.Beginner)
            {
                downPredict = (int)ProcessingEngine.Movement.Up;
            }

            //  double currentUserWeighting = (double)(expertise * currentWeight * downPredict);
            double currentUserWeighting = (double)(expertise * userWeight * perStockExpertise * perStockWeight * downPredict);

            double sumHighVotesMulWeight = 0;
            double sumLowVotesMulWeight = 0;
            int predictedMovement;

            //  queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM Stoocks.StockPrediction WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";
            queryString = @"SELECT SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement FROM stoocks.stockprediction WHERE StockSymbol='" + symbol + "' AND Date='" + tomorrow + "';";

            //  retList = dbOps.QueryMultipleInformationSelect(queryString);
            retList = dbOps.ExecuteReader(queryString);

            if (retList != null && retList.HasRows)
                while (retList.Read())
                {
                    sumHighVotesMulWeight = retList.GetDouble(0);
                    sumLowVotesMulWeight = retList.GetDouble(1);
                    predictedMovement = retList.GetInt32(2);
                }
            retList.Close();

            sumLowVotesMulWeight = sumLowVotesMulWeight + currentUserWeighting;

            if (sumHighVotesMulWeight > Math.Abs(sumLowVotesMulWeight))
            {
                predictedMovement = (int)ProcessingEngine.Movement.Up;
            }
            else
            {
                predictedMovement = (int)ProcessingEngine.Movement.Down;
            }

            //  Populate the UserPrediction and StockPrediction DB.
            //  UserPrediction(Username,PredictedMovement,Symbol,Time,Date)
            //  StockPrediction(StockSymbol,Date,HighVotes,LowVotes,SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement,ActualMovement)

            //  C# Code to Get Date in MySQL Format: System.DateTime.Now.ToString(dateFormatString);
            //  INSERT INTO StockPrediction VALUES ('GOOG','2007-07-31',0,1,0,0,1,1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1;
            //  string queryString = @"UPDATE Stoocks.StockPrediction SET LowVotes = LowVotes + 1 WHERE StockSymbol='" + symbol + "' AND Date='" + DateTime.Now.ToString(dateFormatString) + "';";

            //  queryString = "INSERT INTO Stoocks.UserPrediction VALUES ('" + username + "'," + downPredict + ",'" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + DateTime.Now.ToString(dateFormatString) + "');";
            queryString = "INSERT INTO stoocks.userprediction VALUES ('" + username + "'," + downPredict + ", 0, '" + symbol + "','" + DateTime.Now.ToString("hh:mm:ss") + "','" + tomorrow + "');";
            retVal = dbOps.ExecuteNonQuery(queryString);

            //  queryString = "INSERT INTO Stoocks.StockPrediction VALUES ('" + symbol + "','" + DateTime.Now.ToString(dateFormatString) + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",1) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            retVal = dbOps.ExecuteNonQuery(queryString);
                        
        }

        DownPredictionButton.Enabled = false;
        DownPredictionButton.ImageUrl = @"~/Images/downgraymod.png";
        
        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

        //  Response.Redirect("~/Analysis.aspx?stock=" + symbol);
        StockDetailsLabel.Text = LoadStockDetails(symbol);
    }



    #region Header Links
    protected void HomeLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.HomePageLink);
    }
    protected void InterestsLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.InterestsPageLink);
    }
    protected void PredictionHistoryLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.UserPredictionHistoryPageLink);
    }
    protected void LogoutLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.LogoutPageLink);
    }
    #endregion Header Links


    #region Footer Links
    protected void AboutStoocksLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.AboutPageLink);
    }
    protected void ContactUsLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.ContactUsPageLink);
    }
    protected void PrivacyPolicyLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.PrivacyPolicyPageLink);
    }
    #endregion Footer Links


}
