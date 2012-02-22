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

using System.Collections.Specialized;
using MySql.Data.MySqlClient;

public partial class UserPredictionHistory : System.Web.UI.Page
{
    DBOperations dbOps;
    GUIVariables gui;
    Links links;
    General general;
    ProcessingEngine engine;

    string username;
       
    string dateFormatString = ConfigurationManager.AppSettings["dateFormatString"];
    int maxPredictionHistoryRows = int.Parse(ConfigurationManager.AppSettings["MaxPredictionHistoryRows"]);

    //  int upPredict = 1;
    //  int downPredict = -1;

    string yesterday;
    string today;
    string tomorrow;

    string[] interestedStocks;
    string alreadySelectedStocks;   //  Same as interestedStock, but a single string in CSV Format. Populated in the method ShowALreadySelectedStocks.    
    
    protected void Page_Load(object sender, EventArgs e)
    {
        dbOps = (DBOperations)Application["dbOps"];
        gui = (GUIVariables)Application["gui"];
        links = (Links)Application["links"];
        general = (General)Application["general"];
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
            yesterday = DateTime.Now.AddDays(-1).ToString(dateFormatString);
            today = DateTime.Now.ToString(dateFormatString);
            tomorrow = DateTime.Now.AddDays(+1).ToString(dateFormatString);

            

            UserDetailsLabel.Text = LoadUserData(username);
            LoadUserPredictionHistory();


            interestedStocks = ShowAlreadySelectedStocks(username);
            if (!string.IsNullOrEmpty(alreadySelectedStocks))
            {
                LoadStoockerAccuracy(interestedStocks);                
            }

            
            
            
        }
    }


    public string[] ShowAlreadySelectedStocks(string username)
    {
        string queryString = @"SELECT InterestedStocks FROM stoocks.interests WHERE Username='" + username + "' ;";

        //  alreadySelectedStocks = dbOps.QueryInformationSelect(queryString);
        MySqlDataReader retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
            while (retList.Read())
            {
                alreadySelectedStocks = retList.GetString(0);
            }
        retList.Close();

        //  Error Code for dbOps.QueryInformationSelect() is -1
        if (!string.IsNullOrEmpty(alreadySelectedStocks) && alreadySelectedStocks != "-1")
        {
            string[] splitter = { "," };
            string[] interestedStocks = alreadySelectedStocks.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            return interestedStocks;
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// Load the Data pertaining to the Current User.
    /// This contains Username, Expertise Level and Percentage Predictions.
    /// </summary>
    /// <param name="username">The current username</param>
    /// <returns>string containing the user data.</returns>
    private string LoadUserData(string username)
    {
        string userDetails = "";

        string expertise = "";
        string queryString = "";

        queryString = @"SELECT Expertise FROM stoocks.expertise WHERE Username='" + username + "';";
        int expertiseLevel = dbOps.ExecuteScalar(queryString);

        if (expertiseLevel > 0)
        {
            switch (expertiseLevel)
            {
                case (int)ProcessingEngine.ExpertiseLevel.Beginner:
                    expertise = "Beginner";
                    break;
                case (int)ProcessingEngine.ExpertiseLevel.Intermediate:
                    expertise = "Intermediate";
                    break;
                case (int)ProcessingEngine.ExpertiseLevel.Expert:
                    expertise = "Expert";
                    break;
                case (int)ProcessingEngine.ExpertiseLevel.Master:
                    expertise = "Master";
                    break;
                default:
                    expertise = "";
                    break;
            }

            if (expertise != "")
            {
                userDetails = gui.BoldFontStart
                    + "Name: " + gui.RedFontStart + username + gui.RedFontEnd + "     "
                    + Environment.NewLine
                    + "Expertise: " + gui.RedFontStart + expertise + gui.RedFontEnd
                    + Environment.NewLine
                    + gui.BoldFontEnd;
            }

            //  IF ActualMovement == 0 THEN ActualMovement NOT analyzed yet. Don't take such records into the total count.
            queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND ActualMovement <> 0;";
            int totalUserPredictions = dbOps.ExecuteScalar(queryString);

            queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND PredictedMovement = ActualMovement;";
            int correctUserPredictions = dbOps.ExecuteScalar(queryString);

            float percentageAccuratePredictions = 0;

            if ((totalUserPredictions != -1 && correctUserPredictions != -1) && (totalUserPredictions != 0))
            {
                percentageAccuratePredictions = (float)(((float)correctUserPredictions / (float)totalUserPredictions) * 100);

                //  string absoluteUserPredictionHistoryPageLink = userPredictionHistoryPageLink.StartsWith(@"~\") ? userPredictionHistoryPageLink.Replace(@"~\", "") : userPredictionHistoryPageLink;
                //  string userPredictionHistoryLink = "<a href='" + absoluteUserPredictionHistoryPageLink + "'>" + "View your Prediction History/Accuracy" + "</a>";

                string highlightedPercentageAccuratePredictionsString =
                    percentageAccuratePredictions > 50 ? gui.GreenFontStart + percentageAccuratePredictions.ToString() + "%" + gui.GreenFontEnd : gui.RedFontStart + percentageAccuratePredictions.ToString() + "%" + gui.RedFontEnd;

                userDetails = userDetails + gui.LineBreak
                    + gui.BoldFontStart
                    + "(Your Correct Predictions / Your Total Predictions): " + correctUserPredictions.ToString() + " / " + totalUserPredictions.ToString() + " = " + highlightedPercentageAccuratePredictionsString + gui.LineBreak
                    //  + blueFontStart + userPredictionHistoryLink + blueFontEnd
                    + gui.BoldFontEnd;
            }

        }

        return userDetails;
    }

    private void LoadUserPredictionHistory()
    {
        //  DB Variables.
        string queryString = "";
        //  int retVal = 0;
        //  ArrayList retList;
        MySqlDataReader retList;

        TableItemStyle tableStyle = new TableItemStyle();
        tableStyle.HorizontalAlign = HorizontalAlign.Center;
        tableStyle.VerticalAlign = VerticalAlign.Middle;
        tableStyle.Width = Unit.Pixel(2000);

        TableRow row = new TableRow();
        
        TableCell symbolCell = new TableCell();
        TableCell dateCell = new TableCell();
        TableCell predictedMovementCell = new TableCell();
        TableCell actualMovementCell = new TableCell();
        
                
        ////    Fill the Table Header START     ////

        symbolCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Stock" + gui.BoldFontEnd));
        dateCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Date" + gui.BoldFontEnd));
        predictedMovementCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "You Predicted" + gui.BoldFontEnd));
        actualMovementCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Actual Movement" + gui.BoldFontEnd));
        

        row.Cells.Add(symbolCell);
        row.Cells.Add(dateCell);
        row.Cells.Add(predictedMovementCell);
        row.Cells.Add(actualMovementCell);
        
        
        StockTable.Rows.Add(row);

        StockTable.BorderColor = System.Drawing.Color.Silver;
        StockTable.BorderStyle = BorderStyle.Solid;        
        
        ////    Fill the Table Header END       ////

        //ImageButton upPredictionButton = new ImageButton();
        //ImageButton downPredictionButton = new ImageButton();
        //upPredictionButton.ImageUrl = @"~/Images/upmod.png";
        //downPredictionButton.ImageUrl = @"~/Images/downmod.png";
        //upPredictionButton.Enabled = false;
        //downPredictionButton.Enabled = false;

        Image upPredictionImage = new Image();
        Image downPredictionImage = new Image();
        upPredictionImage.ImageUrl = @"~/Images/upmod.png";
        downPredictionImage.ImageUrl = @"~/Images/downmod.png";

        //  queryString = @"SELECT Symbol, Date, PredictedMovement, ActualMovement FROM Stoocks.UserPrediction WHERE Username = '" + username + "';";
        //  Do not show the entire User Prediction History, show only the most recent n (n between 25 and 100) rows.
        queryString = @"SELECT Symbol, Date, PredictedMovement, ActualMovement FROM stoocks.userprediction WHERE Username = '" + username + "' ORDER BY Date DESC LIMIT " + maxPredictionHistoryRows + ";";
        
        //  retList = dbOps.QueryMultipleRowSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);

        //  for (int i = 0; i < retList.Count; i++)
        if(retList != null && retList.HasRows)
        while(retList.Read())
        {
            row = new TableRow();
            /* 
            //  Before
            //  StringCollection sc = (StringCollection)retList[i];
            //  sc[0] = Symbol, sc[1] = Date, sc[2] = PredictedMovement, sc[3] = ActualMovement
            
            string symbol = sc[0];
            
            string[] splitter = {" "};
            string[] dateSplit = sc[1].Split(splitter,StringSplitOptions.RemoveEmptyEntries);            
            string date = dateSplit[0];            

            string predictedMovement = sc[2];
            string actualMovement = sc[3];
            */

            string symbol = retList.GetString(0);

            string[] splitter = { " " };
            string[] dateSplit = retList.GetString(1).Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            string date = dateSplit[0];

            int predictedMovement = retList.GetInt32(2);
            int actualMovement = retList.GetInt32(3);

            symbolCell = new TableCell();
            dateCell = new TableCell();
            predictedMovementCell = new TableCell();
            actualMovementCell = new TableCell();            

            symbolCell.Text = symbol;
            dateCell.Text = date;

            //upPredictionButton = new ImageButton();
            //downPredictionButton = new ImageButton();
            //upPredictionButton.ImageUrl = @"~/Images/upmod.png";
            //downPredictionButton.ImageUrl = @"~/Images/downmod.png";
            //upPredictionButton.Enabled = false;
            //downPredictionButton.Enabled = false;

            if (predictedMovement == (int)ProcessingEngine.Movement.Up)
            //  if (predictedMovement == "-1")            
            {
                upPredictionImage = new Image();
                upPredictionImage.ImageUrl = @"~/Images/upmod.png";           

                //  predictedMovementCell.Controls.Add(upPredictionButton);
                predictedMovementCell.Controls.Add(upPredictionImage);
            }
            else if (predictedMovement == (int)ProcessingEngine.Movement.Down)
            //  else if (predictedMovement == "-1")
            {
                downPredictionImage = new Image();
                downPredictionImage.ImageUrl = @"~/Images/downmod.png";

                //  predictedMovementCell.Controls.Add(downPredictionButton);
                predictedMovementCell.Controls.Add(downPredictionImage);
            }

            if (actualMovement == (int)ProcessingEngine.Movement.Up)
            //  if (actualMovement == "1")
            {
                upPredictionImage = new Image();
                upPredictionImage.ImageUrl = @"~/Images/upmod.png";
            
                //  actualMovementCell.Controls.Add(upPredictionButton);
                actualMovementCell.Controls.Add(upPredictionImage);
            }
            else if (actualMovement == (int)ProcessingEngine.Movement.Down)
            //  else if (actualMovement == "-1")
            {
                downPredictionImage = new Image();
                downPredictionImage.ImageUrl = @"~/Images/downmod.png";

                //  actualMovementCell.Controls.Add(downPredictionButton);
                actualMovementCell.Controls.Add(downPredictionImage);
            }

            row.Cells.Add(symbolCell);
            row.Cells.Add(dateCell);
            row.Cells.Add(predictedMovementCell);
            row.Cells.Add(actualMovementCell);                    
            
            StockTable.Rows.Add(row);

        }
        retList.Close();

        foreach (TableRow r in StockTable.Rows)
            foreach (TableCell c in r.Cells)
                c.ApplyStyle(tableStyle);

        StockLabel.Text = gui.BoldFontStart + gui.GrayFontStart
                    + "Your Prediction History (Last 25 Predictions)"
                    + gui.GrayFontEnd + gui.BoldFontEnd;
    }
    

    private void LoadStoockerAccuracy(string[] interestedStocks)
    {
        if (interestedStocks != null && interestedStocks.Length > 0)
        {
            //  DB Variables.
            string queryString = "";            

            TableItemStyle tableStyle = new TableItemStyle();
            tableStyle.HorizontalAlign = HorizontalAlign.Center;
            tableStyle.VerticalAlign = VerticalAlign.Middle;
            tableStyle.Width = Unit.Pixel(2000);

            TableRow row = new TableRow();

            TableCell symbolCell = new TableCell();
            //  TableCell yourAccuracyCell = new TableCell();
            TableCell stoockerAccuracyCell = new TableCell();

            symbolCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Stock" + gui.BoldFontEnd));
            //  yourAccuracyCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Your Accuracy" + gui.BoldFontEnd));
            stoockerAccuracyCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Accuracy of Stoocker Algorithm" + gui.BoldFontEnd));

            row.Cells.Add(symbolCell);
            //  row.Cells.Add(yourAccuracyCell);
            row.Cells.Add(stoockerAccuracyCell);

            StoockerAccuracyTable.Rows.Add(row);

            StockTable.BorderColor = System.Drawing.Color.Silver;
            StockTable.BorderStyle = BorderStyle.Solid;

            for (int i = 0; i < interestedStocks.Length; i++)
            {
                row = new TableRow();

                string symbol = interestedStocks[i].Trim().ToUpper();
                
                queryString = "SELECT Count(*) FROM stoocks.stockprediction WHERE stocksymbol='" + symbol + "' AND PredictedMovement = ActualMovement;";
                int correctPredictions = dbOps.ExecuteScalar(queryString);

                queryString = "SELECT Count(*) FROM stoocks.stockprediction WHERE stocksymbol='" + symbol + "' AND ActualMovement <> 0;";
                int totalPredictions = dbOps.ExecuteScalar(queryString);

                if (correctPredictions != -1 && totalPredictions != -1)
                {
                    float symbolAccuracy;

                    if (totalPredictions != 0)
                        symbolAccuracy = (float)(((float)correctPredictions / (float)totalPredictions) * 100);
                    else
                        symbolAccuracy = 0;

                    
                    string highlightedSymbolAccuracy =
                        symbolAccuracy > 50 ? gui.GreenFontStart + symbolAccuracy.ToString() + "%" + gui.GreenFontEnd : gui.RedFontStart + symbolAccuracy.ToString() + "%" + gui.RedFontEnd;

                    string stoockerAccuracyString = "";

                    if (symbolAccuracy != 0)
                        stoockerAccuracyString = "[" + correctPredictions.ToString() + "/" + totalPredictions.ToString() + "]" + "=" + highlightedSymbolAccuracy;
                    else
                        stoockerAccuracyString = gui.GrayFontStart + "-" + gui.GreenFontEnd;

                    symbolCell = new TableCell();
                    //  yourAccuracyCell = new TableCell();
                    stoockerAccuracyCell = new TableCell();

                    symbolCell.Text = symbol;
                    //  yourAccuracyCell.Text = "";
                    stoockerAccuracyCell.Text = stoockerAccuracyString;

                    row.Cells.Add(symbolCell);
                    //  row.Cells.Add(yourAccuracyCell);
                    row.Cells.Add(stoockerAccuracyCell);

                    StoockerAccuracyTable.Rows.Add(row);
                }
            }

            foreach (TableRow r in StoockerAccuracyTable.Rows)
                foreach (TableCell c in r.Cells)
                    c.ApplyStyle(tableStyle);

            StoockerAccuracyLabel.Text = gui.BoldFontStart + gui.GrayFontStart
                    + "Performance of the Stoocker Algorithm over your Portfolio"
                    + gui.GrayFontEnd + gui.BoldFontEnd;

        }
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
