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

public partial class TradeSignal : System.Web.UI.Page
{
    DBOperations dbOps;
    GUIVariables gui;
    RssReader rssReader;
    StockService stockService;
    Links links;
    General general;
    ProcessingEngine engine;

    string username;

    bool isDebug = Convert.ToInt32(ConfigurationManager.AppSettings["isDebug"]) == 0 ? false : true;
    string dateFormatString = ConfigurationManager.AppSettings["dateFormatString"];
    Hashtable indicesTable = (Hashtable)ConfigurationManager.GetSection("CommonIndicesSection");
    bool isGenericNews = Convert.ToInt32(ConfigurationManager.AppSettings["isGenericNews"]) == 0 ? false : true;
    bool isShowUserStockData = Convert.ToInt32(ConfigurationManager.AppSettings["isShowUserStockData"]) == 0 ? false : true;
    bool isUpdateTableCellsOnTheFly = Convert.ToInt32(ConfigurationManager.AppSettings["isUpdateTableCellsOnTheFly"]) == 0 ? false : true;

    int upPredict;
    int downPredict;

    string yesterday;
    string today;
    string tomorrow;

    string[] interestedStocks;
    string alreadySelectedStocks;   //  Same as interestedStock, but a single string in CSV Format. Populated in the method ShowALreadySelectedStocks.    

    protected void Page_Load(object sender, EventArgs e)
    {
        dbOps = (DBOperations)Application["dbOps"];
        rssReader = (RssReader)Application["rssReader"];
        gui = (GUIVariables)Application["gui"];
        stockService = (StockService)Application["stockService"];
        links = (Links)Application["links"];
        general = (General)Application["general"];
        engine = (ProcessingEngine)Application["engine"];

        //  username = Convert.ToString(Session["username"]);
        //  username = "pots";

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

            string welcomeMessage = gui.BoldFontStart + "Welcome to" + gui.StoocksFont + gui.BoldFontEnd;
            WelcomeLabel.Text = welcomeMessage;

            //  Trace.Write("Enter LoadCommonIndicesData");
            LoadCommonIndicesData();
            //  Trace.Write("Exit LoadCommonIndicesData");

            Trace.Write("Enter ShowAlreadySelectedStocks");
            interestedStocks = ShowAlreadySelectedStocks(username);
            Trace.Write("Exit ShowAlreadySelectedStocks");

            if (!string.IsNullOrEmpty(alreadySelectedStocks))
            {
                Trace.Write("Enter LoadYahooFinanceBadgeIFrame");
                LoadYahooFinanceBadgeIFrame(alreadySelectedStocks);
                Trace.Write("Exit LoadYahooFinanceBadgeIFrame");
            }

            //  if (!Page.IsPostBack)
            {
                Trace.Write("Enter LoadUserData");
                UserDetailsLabel.Text = LoadUserData(username);
                Trace.Write("Exit LoadUserData");

                if (isShowUserStockData)
                {
                    Trace.Write("Enter LoadCurrentData");
                    LoadCurrentData();
                    Trace.Write("Exit LoadCurrentData");
                }

                MessageLabel.Text = "";

                Trace.Write("Enter LoadNews");
                string newsFeeds = "";
                if (isGenericNews)      //  Show the same News for all Users, Do Cache, Update every 30 minutes.
                {
                    newsFeeds = rssReader.LoadPerStockNews(alreadySelectedStocks);
                }
                else
                {
                    newsFeeds = rssReader.LoadGenericNews("", false);
                }
                Trace.Write("Exit LoadNews");

                if (!string.IsNullOrEmpty(newsFeeds))
                {
                    NewsMessageLabel.Text = gui.BoldFontStart + "Latest News & Recommended Readings from " + gui.StoocksFont + gui.BoldFontEnd;
                    NewsLabel.Text = newsFeeds;
                }
            }
        }        
    }

    private void LoadCommonIndicesData()
    {
        string commonIndices = "";
        foreach (DictionaryEntry de in indicesTable)
        {
            commonIndices = commonIndices + de.Key + ",";
        }

        if (!string.IsNullOrEmpty(commonIndices))
        {
            TableItemStyle tableStyle = new TableItemStyle();
            tableStyle.HorizontalAlign = HorizontalAlign.Right;
            tableStyle.VerticalAlign = VerticalAlign.Middle;
            tableStyle.Width = Unit.Pixel(200);

            TableRow row = new TableRow();

            TableCell symbolCell = new TableCell();
            TableCell priceCell = new TableCell();
            TableCell changeCell = new TableCell();

            CommonIndicesTable.BorderColor = System.Drawing.Color.Silver;
            CommonIndicesTable.BorderStyle = BorderStyle.Solid;

            string stock = "";
            string currentPrice = "";
            string change = "";

            //  DataSet ds = stockService.YahooCSVGetQuotesMatrix(commonIndices);
            DataSet ds = GetCommonIndicesQuotesMatrix(commonIndices, false);

            if (ds != null)
            {
                //  Column Namesof the DataSet: [Stock | Price | Change | PreviousClose]
                foreach (DataRow dr in ds.Tables["StockData"].Rows)
                {
                    if (indicesTable.ContainsKey(dr["stock"].ToString()))
                    {
                        stock = indicesTable[dr["Stock"].ToString()].ToString(); // +"(" + dr["Stock"].ToString() + ")";
                        currentPrice = dr["Price"].ToString();
                        change = dr["Change"].ToString();

                        row = new TableRow();

                        symbolCell = new TableCell();
                        symbolCell.Text = stock;

                        priceCell = new TableCell();
                        priceCell.Text = currentPrice;

                        changeCell = new TableCell();
                        if (change.StartsWith("-"))
                        {
                            //  changeCell.BackColor = System.Drawing.Color.Red;
                            change = gui.RedFontStart + change + gui.RedFontEnd;
                        }
                        else
                        {
                            //  changeCell.BackColor = System.Drawing.Color.LightGreen;
                            change = gui.GreenFontStart + change + gui.GreenFontEnd;
                        }
                        changeCell.Text = change;

                        row.Cells.Add(symbolCell);
                        row.Cells.Add(priceCell);
                        row.Cells.Add(changeCell);

                        CommonIndicesTable.Rows.Add(row);
                    }
                }
            }

            foreach (TableRow r in CommonIndicesTable.Rows)
                foreach (TableCell c in r.Cells)
                    c.ApplyStyle(tableStyle);

            IndicesTimeLabel.Text = gui.GrayFontStart + stockService.GetTimeOfLastTrade("^DJI") + gui.GrayFontEnd;
        }
    }

    private DataSet GetCommonIndicesQuotesMatrix(string commonIndices, bool isByPassCache)
    {
        Trace.Write("Enter GetCommonIndicesQuotesMatrix");
        DataSet ds = Cache["commonIndices"] as DataSet;
        if (isByPassCache || (ds == null))
        {
            Trace.Write("Enter YahooCSVGetQuotesMatrix");
            ds = stockService.YahooCSVGetQuotesMatrix(commonIndices);
            Cache.Insert("commonIndices", ds, null, DateTime.Now.AddMinutes(20), System.Web.Caching.Cache.NoSlidingExpiration);
            Trace.Write("Exit YahooCSVGetQuotesMatrix");
        }
        Trace.Write("Exit GetCommonIndicesQuotesMatrix");
        return ds;
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
                    + gui.BoldFontEnd;
            }

            #region PerUserAnalysisCode
            //  //  Now Used in UserPredictionHistory.aspx (since December 18 2007)

            //  //  IF ActualMovement == 0 THEN ActualMovement NOT analyzed yet. Don't take such records into the total count.
            //  queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND ActualMovement <> 0;";
            //  int totalUserPredictions = dbOps.ExecuteScalar(queryString);

            //  queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND PredictedMovement = ActualMovement;";
            //  int correctUserPredictions = dbOps.ExecuteScalar(queryString);

            //  float percentageAccuratePredictions = 0;

            //  if ((totalUserPredictions != -1 && correctUserPredictions != -1) && (totalUserPredictions != 0))
            //  {
            //      percentageAccuratePredictions = (float)(((float)correctUserPredictions / (float)totalUserPredictions) * 100);

            //      //  string absoluteUserPredictionHistoryPageLink = userPredictionHistoryPageLink.StartsWith(@"~\") ? userPredictionHistoryPageLink.Replace(@"~\", "") : userPredictionHistoryPageLink;
            //      //  string userPredictionHistoryLink = "<a href='" + absoluteUserPredictionHistoryPageLink + "'>" + "View your Prediction History/Accuracy" + "</a>";

            //      string highlightedPercentageAccuratePredictionsString =
            //            percentageAccuratePredictions > 50 ? gui.GreenFontStart + percentageAccuratePredictions.ToString() + "%" + gui.GreenFontEnd : gui.RedFontStart + percentageAccuratePredictions.ToString() + "%" + gui.RedFontEnd;

            //      userDetails = userDetails + gui.LineBreak
            //        + gui.BoldFontStart
            //        + "(Your Correct Predictions / Your Total Predictions): " + correctUserPredictions.ToString() + " / " + totalUserPredictions.ToString() + " = " + highlightedPercentageAccuratePredictionsString + gui.LineBreak
            //        //  + blueFontStart + userPredictionHistoryLink + blueFontEnd
            //        + gui.BoldFontEnd;
            //  }
            #endregion PerUserAnalysisCode

        }
        return userDetails;
    }

    //  New LoadCurrentData. Test Mode. Uses YahooCSVGetQuotesMatrix
    private void LoadCurrentData()
    {
        if (interestedStocks != null && interestedStocks.Length > 0)
        {
            TableItemStyle tableStyle = new TableItemStyle();
            tableStyle.HorizontalAlign = HorizontalAlign.Center;
            tableStyle.VerticalAlign = VerticalAlign.Middle;
            tableStyle.Width = Unit.Pixel(2000);

            PortfolioLabel.Text = gui.BoldFontStart + gui.RedFontStart + username + "'s " + gui.RedFontEnd + "Prediction Portfolio:" + gui.BoldFontEnd;

            TableRow row = new TableRow();

            TableCell symbolCell = new TableCell();
            TableCell priceCell = new TableCell();
            TableCell changeCell = new TableCell();
            TableCell previousCloseCell = new TableCell();
            TableCell upVotesVSdownVotesCell = new TableCell();
            TableCell predictionCell = new TableCell();
            TableCell currentUserPredictedValueCell = new TableCell();
            //  TableCell analysisCell = new TableCell();

            ////    Fill the Table Header START     ////
                        
            symbolCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Stock Analysis" + gui.BoldFontEnd));
            priceCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Current Price" + gui.BoldFontEnd));
            changeCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Change" + gui.BoldFontEnd));
            previousCloseCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Previous Close" + gui.BoldFontEnd));
            upVotesVSdownVotesCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Number of Recommendations " 
                + "[" + gui.GreenFontStart + "Buy" + gui.GreenFontEnd + "|" + gui.GrayFontStart + "Hold" + gui.GrayFontEnd + "|" + gui.RedFontStart + "Sell" + gui.RedFontEnd + "]" + gui.BoldFontEnd));
            currentUserPredictedValueCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "What Other Users Recommend" + gui.BoldFontEnd));
                        
            string clickToRecommend = gui.BoldFontStart 
                + "What" + gui.HTMLSpace + "Do" + gui.HTMLSpace + "You" 
                + gui.LineBreak + "Recommend"
                + gui.BoldFontEnd;            
            predictionCell.Controls.Add(new LiteralControl(clickToRecommend));
            //  predictionCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Click to Recommend" + gui.BoldFontEnd));
            
            //  analysisCell.Controls.Add(new LiteralControl(gui.BoldFontStart + "Analysis" + gui.BoldFontEnd));



            row.Cells.Add(symbolCell);
            row.Cells.Add(priceCell);
            row.Cells.Add(previousCloseCell);
            row.Cells.Add(changeCell);
            row.Cells.Add(upVotesVSdownVotesCell);
            row.Cells.Add(currentUserPredictedValueCell);
            row.Cells.Add(predictionCell);
            //  row.Cells.Add(analysisCell);

            StockTable.Rows.Add(row);

            StockTable.BorderColor = System.Drawing.Color.Silver;
            StockTable.BorderStyle = BorderStyle.Solid;

            ////    Fill the Table Header END       ////

            //ListBox TradeSignalLB = new ListBox();
            //TradeSignalLB.Items.Add(new ListItem(ProcessingEngine.TradeSignal.Buy.ToString(), ProcessingEngine.TradeSignal.Buy.ToString()));
            //TradeSignalLB.Items.Add(new ListItem(ProcessingEngine.TradeSignal.Hold.ToString(), ProcessingEngine.TradeSignal.Hold.ToString()));
            //TradeSignalLB.Items.Add(new ListItem(ProcessingEngine.TradeSignal.Sell.ToString(), ProcessingEngine.TradeSignal.Sell.ToString()));                        
            //TradeSignalLB.SelectedIndexChanged += new EventHandler(TradeSignalLB_SelectedIndexChanged);
            //TradeSignalLB.SelectionMode = ListSelectionMode.Single;                   
            

            //  ImageButton upPredictionButton = new ImageButton();
            //  ImageButton downPredictionButton = new ImageButton();
            //  upPredictionButton.ImageUrl = @"~/Images/upmod.png";
            //  downPredictionButton.ImageUrl = @"~/Images/downmod.png";
            //  upPredictionButton.Click += new ImageClickEventHandler(this.UpPredictionButton_Click);
            //  downPredictionButton.Click += new ImageClickEventHandler(this.DownPredictionButton_Click);

            //Literal SpaceLiteral = new Literal();
            //SpaceLiteral.Text = gui.HTMLSpace;

            LinkButton BuyButton = new LinkButton();
            LinkButton HoldButton = new LinkButton();
            LinkButton SellButton = new LinkButton();
                        
            BuyButton.Text  = gui.GreenFontStart + ProcessingEngine.TradeSignal.Buy.ToString() + gui.GreenFontEnd;
            HoldButton.Text = gui.GrayFontStart + ProcessingEngine.TradeSignal.Hold.ToString() + gui.GrayFontEnd;
            SellButton.Text = gui.RedFontStart + ProcessingEngine.TradeSignal.Sell.ToString() + gui.RedFontEnd;

            //BuyButton.Attributes.Add("Font-Underline", "false");
            //HoldButton.Attributes.Add("Font-Underline", "false");
            //SellButton.Attributes.Add("Font-Underline", "false");

            BuyButton.Font.Underline = false;
            HoldButton.Font.Underline = false;
            SellButton.Font.Underline = false;

            //  <td style="background-color:#c8d2ff; width: 100px; height: 21px" onmouseover="this.style.backgroundColor='#e8e8ff'" onmouseout="this.style.backgroundColor='#c8d2ff'">
            //  <asp:LinkButton ID="HomeLinkButton" runat="server" OnClick="HomeLinkButton_Click" Font-Underline="false">Home</asp:LinkButton></td>

            BuyButton.Attributes.Add("onmouseover", "this.style.backgroundColor='#e8e8ff'");
            HoldButton.Attributes.Add("onmouseover", "this.style.backgroundColor='#e8e8ff'");
            SellButton.Attributes.Add("onmouseover", "this.style.backgroundColor='#e8e8ff'");

            BuyButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");
            HoldButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");
            SellButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");       
                                    
            BuyButton.Click += new EventHandler(BuyButton_Click);
            HoldButton.Click += new EventHandler(HoldButton_Click);
            SellButton.Click += new EventHandler(SellButton_Click);
            

            string stock = "";
            string currentPrice = "";
            string change = "";
            string previousClose = "";

            string queryString = "";

            //  DataSet ds = stockService.YahooCSVGetQuotesMatrix(alreadySelectedStocks);
            DataSet ds = GetUserStockQuotesMatrix(false);

            if (ds != null)
            {
                //  Column Namesof the DataSet: [Stock | Price | Change | PreviousClose]
                foreach (DataRow dr in ds.Tables["StockData"].Rows)
                {
                    stock = dr["Stock"].ToString();
                    currentPrice = dr["Price"].ToString();
                    change = dr["Change"].ToString();
                    previousClose = dr["PreviousClose"].ToString();

                    row = new TableRow();

                    symbolCell = new TableCell();
                    symbolCell.Text = stock;

                    HyperLink symbolLink = new HyperLink();
                    symbolLink.NavigateUrl = "~/Analysis.aspx?stock=" + stock;
                    symbolLink.Text = stock;
                    symbolCell.Controls.Add(symbolLink);

                    //  Fetch the Current Stock Price, Change, Previous Close Price.
                    //  stockService.XIgniteGetQuoteData(interestedStocks[i], out currentPrice, out change, out previousClose);            
                    //  stockService.YahooCSVGetQuoteData(interestedStocks[i], out currentPrice, out change, out previousClose);

                    priceCell = new TableCell();
                    priceCell.Text = currentPrice;

                    previousCloseCell = new TableCell();
                    previousCloseCell.Text = previousClose;

                    //if (!change.StartsWith("-")) //   Has been removed since the YahooCSV Comes with a + Sign.
                    //{
                    //    change = "+" + change;
                    //}

                    changeCell = new TableCell();
                    if (change.StartsWith("-"))
                    {
                        //  changeCell.BackColor = System.Drawing.Color.Red;
                        change = gui.RedFontStart + change + gui.RedFontEnd;
                    }
                    else
                    {
                        //  changeCell.BackColor = System.Drawing.Color.LightGreen;
                        change = gui.GreenFontStart + change + gui.GreenFontEnd;
                    }
                    changeCell.Text = change;


                    //  Fetch HighVotes | LowVotes per Stock for Tomorrow.
                    int currentPrediction = 0;
                    queryString = @"SELECT PredictedMovement, HighVotes, LowVotes  FROM stoocks.stockprediction WHERE StockSymbol='" + stock + "' AND Date='" + tomorrow + "';";

                    MySqlDataReader retList = dbOps.ExecuteReader(queryString);

                    currentUserPredictedValueCell = new TableCell();
                    upVotesVSdownVotesCell = new TableCell();

                    string currentUserPredictedValueCellString = "";
                    string upVotesVSdownVotesCellString = "";

                    if (retList != null && retList.HasRows)
                        while (retList.Read())
                        {
                            currentPrediction = retList.GetInt32(0);
                            string highVotes = retList.GetString(1);
                            string lowVotes = retList.GetString(2);

                            if (currentPrediction > 0)
                            {
                                currentUserPredictedValueCellString = gui.BoldFontStart + gui.GreenFontStart + "Up" + gui.GreenFontEnd + gui.BoldFontEnd;
                            }
                            else
                            {
                                currentUserPredictedValueCellString = gui.BoldFontStart + gui.RedFontStart + "Down" + gui.RedFontEnd + gui.BoldFontEnd;
                            }

                            upVotesVSdownVotesCellString = gui.BoldFontStart
                                + gui.GreenFontStart + highVotes + gui.GreenFontEnd
                                + " | "
                                + gui.RedFontStart + lowVotes + gui.RedFontEnd
                                + gui.BoldFontEnd;
                        }
                    retList.Close();

                    currentUserPredictedValueCell.Text = currentUserPredictedValueCellString;
                    upVotesVSdownVotesCell.Text = upVotesVSdownVotesCellString;

                    //  priceCell.Controls.Add(new LiteralControl(currentPrice));

                    //TradeSignalLB = new ListBox();                    
                    //TradeSignalLB.Items.Add(new ListItem(ProcessingEngine.TradeSignal.Buy.ToString(), ProcessingEngine.TradeSignal.Buy.ToString()));
                    //TradeSignalLB.Items.Add(new ListItem(ProcessingEngine.TradeSignal.Hold.ToString(), ProcessingEngine.TradeSignal.Hold.ToString()));
                    //TradeSignalLB.Items.Add(new ListItem(ProcessingEngine.TradeSignal.Sell.ToString(), ProcessingEngine.TradeSignal.Sell.ToString()));
                    //TradeSignalLB.SelectedIndexChanged += new EventHandler(TradeSignalLB_SelectedIndexChanged);                    
                    //TradeSignalLB.SelectionMode = ListSelectionMode.Single;                   

                    //predictionCell = new TableCell();
                    //predictionCell.Controls.Add(TradeSignalLB);



                    //upPredictionButton = new ImageButton();
                    //downPredictionButton = new ImageButton();
                    //upPredictionButton.ImageUrl = @"~/Images/upmod.png";
                    //downPredictionButton.ImageUrl = @"~/Images/downmod.png";
                    //upPredictionButton.Click += new ImageClickEventHandler(this.UpPredictionButton_Click);
                    //downPredictionButton.Click += new ImageClickEventHandler(this.DownPredictionButton_Click);
                    
                    //predictionCell = new TableCell();
                    //predictionCell.Controls.Add(upPredictionButton);
                    //predictionCell.Controls.Add(downPredictionButton);
                                        

                    BuyButton = new LinkButton();
                    HoldButton = new LinkButton();
                    SellButton = new LinkButton();

                    BuyButton.Text = gui.GreenFontStart + ProcessingEngine.TradeSignal.Buy.ToString() + gui.GreenFontEnd;
                    HoldButton.Text = gui.GrayFontStart + ProcessingEngine.TradeSignal.Hold.ToString() + gui.GrayFontEnd;
                    SellButton.Text = gui.RedFontStart + ProcessingEngine.TradeSignal.Sell.ToString() + gui.RedFontEnd;


                    BuyButton.Font.Underline = false;
                    HoldButton.Font.Underline = false;
                    SellButton.Font.Underline = false;

                    //BuyButton.Attributes.Add("Font-Underline","false");
                    //HoldButton.Attributes.Add("Font-Underline", "false");
                    //SellButton.Attributes.Add("Font-Underline", "false");

                    BuyButton.Attributes.Add("onmouseover", "this.style.backgroundColor='#e8e8ff'");
                    HoldButton.Attributes.Add("onmouseover", "this.style.backgroundColor='#e8e8ff'");
                    SellButton.Attributes.Add("onmouseover", "this.style.backgroundColor='#e8e8ff'");

                    BuyButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");
                    HoldButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");
                    SellButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");    

                    BuyButton.Click += new EventHandler(BuyButton_Click);
                    HoldButton.Click += new EventHandler(BuyButton_Click);
                    SellButton.Click += new EventHandler(BuyButton_Click);

                    predictionCell = new TableCell();
                    predictionCell.Controls.Add(BuyButton);
                    predictionCell.Controls.Add(new LiteralControl(gui.HTMLSpace));                      
                    predictionCell.Controls.Add(HoldButton);
                    predictionCell.Controls.Add(new LiteralControl(gui.HTMLSpace));           
                    predictionCell.Controls.Add(SellButton);

                    //  //  Analysis now covered in SymbolCell (i.e. Cell[0]);
                    //HyperLink analysisLink = new HyperLink();
                    //analysisLink.NavigateUrl = "~/Analysis.aspx?stock=" + interestedStocks[i];
                    //analysisLink.Text = interestedStocks[i];

                    //analysisCell = new TableCell();
                    //analysisCell.Controls.Add(analysisLink);

                    row.Cells.Add(symbolCell);
                    row.Cells.Add(priceCell);
                    row.Cells.Add(previousCloseCell);
                    row.Cells.Add(changeCell);
                    row.Cells.Add(upVotesVSdownVotesCell);
                    row.Cells.Add(currentUserPredictedValueCell);
                    row.Cells.Add(predictionCell);
                    //  row.Cells.Add(analysisCell);

                    StockTable.Rows.Add(row);
                }
            }

            //  ds.Dispose();
            //  ds = null;

            foreach (TableRow r in StockTable.Rows)
                foreach (TableCell c in r.Cells)
                    c.ApplyStyle(tableStyle);

            TimeLabel.Text = gui.GrayFontStart + stockService.GetTimeOfLastTrade("YHOO") + gui.GrayFontEnd;

            ExplanationLabel.Text = gui.SmallFontStart + gui.GrayFontStart
                        + "The 'Users Recommend' & 'Number of Predictions' columns show Stock-Predictions from Users for the Next Business Day"
                        + gui.GrayFontEnd + gui.SmallFontEnd;
        }
        else
        {
            string interestsLink = "<a href='Interests.aspx'>Interests</a>";

            PortfolioLabel.Text = gui.BlueFontStart
                + "You have not yet added any stocks to your portfolio."
                + gui.LineBreak
                + "Please visit the " + interestsLink + " Page to add your favorite stocks to your account and start " + gui.BlueFontEnd
                + gui.BoldFontStart + gui.RedFontStart + "Stoocking" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

            RefreshLink.Visible = false;

        }

    }

    private DataSet GetUserStockQuotesMatrix(bool isByPassCache)
    {
        DataSet ds = new DataSet(); //  Will contain the final result DataSet.

        if (isByPassCache)  //  Do not go through the Cache.
        {
            ds = stockService.YahooCSVGetQuotesMatrix(alreadySelectedStocks);
        }
        else    //  Go through the Cache.
        {
            //  Trace.Write("Enter GetUserStockQuotesMatrix");

            string stocksNotInCache = "";
            string[] valueSplitter = { "," };

            DataSet dsCache = new DataSet();    //  Will contain the DataSet of stocks which are present in the Cache.
            DataTable dtCache = new DataTable("StockData");
            DataColumn dcCache;
            dcCache = dtCache.Columns.Add("Stock", System.Type.GetType("System.String"));
            dcCache = dtCache.Columns.Add("Price", System.Type.GetType("System.String"));
            dcCache = dtCache.Columns.Add("Change", System.Type.GetType("System.String"));
            dcCache = dtCache.Columns.Add("PreviousClose", System.Type.GetType("System.String"));

            for (int i = 0; i < interestedStocks.Length; i++)
            {
                if (Cache[interestedStocks[i]] == null) //  IF Cache does not contain the current stock symbol.
                {
                    if (string.IsNullOrEmpty(stocksNotInCache))
                    {
                        stocksNotInCache = interestedStocks[i];
                    }
                    else
                    {
                        stocksNotInCache = stocksNotInCache + "," + interestedStocks[i];
                    }
                }
                else    //  IF Cache contains the current stock symbol.
                {
                    DataRow drCache = dtCache.NewRow();

                    string[] values = Cache[interestedStocks[i]].ToString().Split(valueSplitter, StringSplitOptions.RemoveEmptyEntries);
                    double price = Convert.ToDouble(values[0]);
                    double change = Convert.ToDouble(values[1]);

                    drCache["Stock"] = interestedStocks[i].ToUpper();
                    drCache["Price"] = values[0];
                    drCache["Change"] = values[1];
                    drCache["PreviousClose"] = Convert.ToString(price - change);

                    dtCache.Rows.Add(drCache);
                }
            }
            dsCache.Tables.Add(dtCache);

            if (!string.IsNullOrEmpty(stocksNotInCache)) //  If every stock was not found in the Cache, call the StockService
            {
                Trace.Write("Enter YahooCSVGetQuotesMatrix");
                ds = stockService.YahooCSVGetQuotesMatrix(stocksNotInCache);
                Trace.Write("Exit YahooCSVGetQuotesMatrix");
                foreach (DataRow dr in ds.Tables["StockData"].Rows)
                {
                    string key = dr["stock"].ToString().ToUpper();
                    string value = dr["Price"].ToString() + "," + dr["Change"].ToString();
                    Cache.Insert(key, value, null, DateTime.Now.AddMinutes(20), System.Web.Caching.Cache.NoSlidingExpiration);
                }
            }
            else
            {
                //  ds does not contain a DataTable. Add a DataTable.
                DataTable dt = new DataTable("StockData");
                DataColumn dc;
                dc = dt.Columns.Add("Stock", System.Type.GetType("System.String"));
                dc = dt.Columns.Add("Price", System.Type.GetType("System.String"));
                dc = dt.Columns.Add("Change", System.Type.GetType("System.String"));
                dc = dt.Columns.Add("PreviousClose", System.Type.GetType("System.String"));

                ds.Tables.Add(dt);
            }

            //  Merge the Stocks 
            //  (1) Which were present in the Cache and 
            //  (2) Which were not present in the Cache and had to be retrieved via the StockService.
            ds.Merge(dsCache);

            //  Trace.Write("Exit GetUserStockQuotesMatrix");
        }
        return ds;
    }

    protected void BuyButton_Click(object sender, EventArgs e)
    {
        LinkButton iButton = (LinkButton)sender;
        TableRow row = (TableRow)iButton.Parent.Parent;
        string symbol = row.Cells[0].Text;
        string price = row.Cells[1].Text;

        string queryString = "";
        int retVal = 0;

        MySqlDataReader retList;

        int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
        double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.

        iButton.Enabled = false;        

        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

    }


    protected void HoldButton_Click(object sender, EventArgs e)
    {
        LinkButton iButton = (LinkButton)sender;
        TableRow row = (TableRow)iButton.Parent.Parent;
        string symbol = row.Cells[0].Text;
        string price = row.Cells[1].Text;

        string queryString = "";
        int retVal = 0;

        MySqlDataReader retList;

        int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
        double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.

        iButton.Enabled = false;        

        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

    }

    protected void SellButton_Click(object sender, EventArgs e)
    {
        LinkButton iButton = (LinkButton)sender;
        TableRow row = (TableRow)iButton.Parent.Parent;
        string symbol = row.Cells[0].Text;
        string price = row.Cells[1].Text;

        string queryString = "";
        int retVal = 0;

        MySqlDataReader retList;

        int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise.
        double userWeight = engine.InitialUserWeight;   //  Default CurrentWeight.

        iButton.Enabled = false;        

        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;

    }
    
    
    protected void UpPredictionButton_Click(object sender, EventArgs e)
    {
        ImageButton iButton = (ImageButton)sender;
        TableRow row = (TableRow)iButton.Parent.Parent;
        string symbol = row.Cells[0].Text;
        string price = row.Cells[1].Text;

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



            //  Debug
            if (isDebug)
            {
                int vote = 1;
                Response.Write(symbol + "  " + vote.ToString() + "  " + tomorrow + "  " + expertise.ToString() + "  " + userWeight.ToString() + "  " + sumHighVotesMulWeight.ToString() + "  " + sumLowVotesMulWeight.ToString() + "  " + predictedMovement.ToString() + "  " + price);
            }
        }

        iButton.Enabled = false;
        iButton.ImageUrl = @"~/Images/upgraymod.png";

        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;


        if (isUpdateTableCellsOnTheFly)
        {
            //  row.Cells[4].Text = "+22";
            string stock = symbol;

            //  Fetch HighVotes | LowVotes per Stock for Tomorrow.
            int currentPrediction = 0;
            queryString = @"SELECT PredictedMovement, HighVotes, LowVotes  FROM stoocks.stockprediction WHERE StockSymbol='" + stock + "' AND Date='" + tomorrow + "';";

            retList = dbOps.ExecuteReader(queryString);

            string currentUserPredictedValueCellString = "";
            string upVotesVSdownVotesCellString = "";

            if (retList != null && retList.HasRows)
                while (retList.Read())
                {
                    currentPrediction = retList.GetInt32(0);
                    string highVotes = retList.GetString(1);
                    string lowVotes = retList.GetString(2);

                    if (currentPrediction > 0)
                    {
                        currentUserPredictedValueCellString = gui.BoldFontStart + gui.GreenFontStart + "Up" + gui.GreenFontEnd + gui.BoldFontEnd;
                    }
                    else
                    {
                        currentUserPredictedValueCellString = gui.BoldFontStart + gui.RedFontStart + "Down" + gui.RedFontEnd + gui.BoldFontEnd;
                    }

                    upVotesVSdownVotesCellString = gui.BoldFontStart
                        + gui.GreenFontStart + highVotes + gui.GreenFontEnd
                        + " | "
                        + gui.RedFontStart + lowVotes + gui.RedFontEnd
                        + gui.BoldFontEnd;
                }
            retList.Close();

            row.Cells[4].Text = upVotesVSdownVotesCellString;
            row.Cells[5].Text = currentUserPredictedValueCellString;

        }

    }

    protected void DownPredictionButton_Click(object sender, EventArgs e)
    {
        ImageButton iButton = (ImageButton)sender;
        TableRow row = (TableRow)iButton.Parent.Parent;
        string symbol = row.Cells[0].Text;
        string price = row.Cells[1].Text;

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

            queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",0) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
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
            queryString = "INSERT INTO stoocks.stockprediction VALUES ('" + symbol + "','" + tomorrow + "',0,1,0," + sumLowVotesMulWeight + "," + predictedMovement + ",0) ON DUPLICATE KEY UPDATE LowVotes=LowVotes+1, SumLowVotesMulWeight=" + sumLowVotesMulWeight + ",PredictedMovement=" + predictedMovement + ";";
            retVal = dbOps.ExecuteNonQuery(queryString);

            //  Debug
            if (isDebug)
            {
                int vote = 0;
                Response.Write(symbol + " " + vote.ToString() + " " + tomorrow + " " + expertise.ToString() + " " + userWeight.ToString() + " " + sumHighVotesMulWeight.ToString() + " " + sumLowVotesMulWeight.ToString() + " " + predictedMovement.ToString() + " " + price);
            }
        }

        iButton.Enabled = false;
        iButton.ImageUrl = @"~/Images/downgraymod.png";

        MessageLabel.Text = gui.BoldFontStart + "Your Prediction has been submitted. "
            + gui.RedFontStart + "Thank You" + gui.RedFontEnd + "!" + gui.BoldFontEnd;


        if (isUpdateTableCellsOnTheFly)
        {
            //  row.Cells[4].Text = "-22";
            string stock = symbol;

            //  Fetch HighVotes | LowVotes per Stock for Tomorrow.
            int currentPrediction = 0;
            queryString = @"SELECT PredictedMovement, HighVotes, LowVotes  FROM stoocks.stockprediction WHERE StockSymbol='" + stock + "' AND Date='" + tomorrow + "';";

            retList = dbOps.ExecuteReader(queryString);

            string currentUserPredictedValueCellString = "";
            string upVotesVSdownVotesCellString = "";

            if (retList != null && retList.HasRows)
                while (retList.Read())
                {
                    currentPrediction = retList.GetInt32(0);
                    string highVotes = retList.GetString(1);
                    string lowVotes = retList.GetString(2);

                    if (currentPrediction > 0)
                    {
                        currentUserPredictedValueCellString = gui.BoldFontStart + gui.GreenFontStart + "Up" + gui.GreenFontEnd + gui.BoldFontEnd;
                    }
                    else
                    {
                        currentUserPredictedValueCellString = gui.BoldFontStart + gui.RedFontStart + "Down" + gui.RedFontEnd + gui.BoldFontEnd;
                    }

                    upVotesVSdownVotesCellString = gui.BoldFontStart
                        + gui.GreenFontStart + highVotes + gui.GreenFontEnd
                        + " | "
                        + gui.RedFontStart + lowVotes + gui.RedFontEnd
                        + gui.BoldFontEnd;
                }
            retList.Close();

            row.Cells[4].Text = upVotesVSdownVotesCellString;
            row.Cells[5].Text = currentUserPredictedValueCellString;


        }
    }

    private void LoadYahooFinanceBadgeIFrame(string alreadySelectedStocks)
    {
        YahooFinanceBadge yfb = new YahooFinanceBadge();
        if (yfb.IsShowYFB)
        {
            YahooFinanceBadgeIFramePlaceHolder.Controls.Add(yfb.CreateYBF(alreadySelectedStocks, 2, true, 0, "no", 300, 500));
        }
    }            
}
