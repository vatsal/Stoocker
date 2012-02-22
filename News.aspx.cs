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
using System.Collections.Generic;

public partial class News : System.Web.UI.Page
{
    DBOperations dbOps;
    GUIVariables gui;
    RssReader rssReader;
    StockService stockService;
    Links links;
    Logger log;
    
    Hashtable newsFeedsTable = (Hashtable)ConfigurationManager.GetSection("NewsFeedsSection");
    Hashtable perStockNewsFeedsTable = (Hashtable)ConfigurationManager.GetSection("PerStockNewsFeedsSection");
    Hashtable ipoNewsFeedsTable = (Hashtable)ConfigurationManager.GetSection("IPONewsFeedsSection");

    int newsRefreshRate = Convert.ToInt32(ConfigurationManager.AppSettings["newsRefreshRate"]);

    /*
    string yahooNewsFeedURL = "";
    string googleNewsFeedURL = "";
    string msnNewsFeedURL = "";
    string wsjNewsFeedURL = "";
    */

    StringCollection newsSources = new StringCollection();
    StringCollection newsFeeds = new StringCollection();

    string username = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        dbOps = (DBOperations)Application["dbOps"];
        rssReader = (RssReader)Application["rssReader"];
        gui = (GUIVariables)Application["gui"];
        stockService = (StockService)Application["stockService"];
        links = (Links)Application["links"];
        log = (Logger)Application["log"];


        NewsSearchLabel.Text = gui.GreenFontStart + "Enter a Stock Symbol: " + gui.GreenFontEnd;
        

        if (Request.Cookies["stoockerCookie"] != null)
        {
            HttpCookie stoockerCookie = Request.Cookies["stoockerCookie"];
            username = dbOps.Decrypt(stoockerCookie["username"].ToString().Trim());

            GoBackLink.Text = "Home Page";
            LogoutLink.Visible = true;
        }
        else
        {
            GoBackLink.Text = "Login";
            LogoutLink.Visible = false;
        }


        //  if (!Page.IsPostBack)
        {
            GetNews(newsFeedsTable);
            RenderNews();

            StockTable.Visible = false;
        }
    }


    private void GetNews(Hashtable configTableToUse)
    {
        newsFeeds.Clear();
        newsSources.Clear();

        string newsFeed = "";

        foreach (DictionaryEntry de in configTableToUse)
        {
            string source = de.Key.ToString().Trim();
            string feedURL = de.Value.ToString().Trim();

            if (Cache[source] == null)
            {
                newsFeed = rssReader.LoadGenericNews(feedURL, true);
                if (!string.IsNullOrEmpty(newsFeed))
                {
                    Cache.Insert(source, newsFeed, null, DateTime.Now.AddMinutes(newsRefreshRate), System.Web.Caching.Cache.NoSlidingExpiration);
                }
            }
            else
            {
                newsFeed = Convert.ToString(Cache[source]);
            }
            
            if (!string.IsNullOrEmpty(newsFeed))    //  Only add the Feed if its not empty.
            {
                newsFeeds.Add(newsFeed);
                newsSources.Add(source);
            }
        }        
    }

    private void GetNews(string ticker)
    {

    }

    private void RenderNews()
    {
        NewsMessageLabelA.Text = NewsMessageLabelB.Text = NewsMessageLabelC.Text = NewsMessageLabelD.Text = NewsMessageLabelE.Text = NewsMessageLabelF.Text  = string.Empty;
        NewsLabelA.Text = NewsLabelB.Text = NewsLabelC.Text = NewsLabelD.Text = NewsLabelE.Text = NewsLabelF.Text = string.Empty;

        if (newsFeeds.Count == 6)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[0] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = newsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[1] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = newsFeeds[1];

            NewsMessageLabelC.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[2] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelC.Text = newsFeeds[2];

            NewsMessageLabelD.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[3] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelD.Text = newsFeeds[3];

            NewsMessageLabelE.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[4] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelE.Text = newsFeeds[4];

            NewsMessageLabelF.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[5] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelF.Text = newsFeeds[5];
        }
        if (newsFeeds.Count == 5)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[0] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = newsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[1] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = newsFeeds[1];

            NewsMessageLabelC.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[2] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelC.Text = newsFeeds[2];

            NewsMessageLabelD.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[3] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelD.Text = newsFeeds[3];

            NewsMessageLabelE.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[4] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelE.Text = newsFeeds[4];
        }
        else if (newsFeeds.Count == 4)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[0] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = newsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[1] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = newsFeeds[1];

            NewsMessageLabelC.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[2] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelC.Text = newsFeeds[2];

            NewsMessageLabelD.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[3] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelD.Text = newsFeeds[3];
        }
        if (newsFeeds.Count == 3)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[0] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = newsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[1] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = newsFeeds[1];

            NewsMessageLabelC.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[2] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelC.Text = newsFeeds[2];           
        }
        else if (newsFeeds.Count == 2)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[0] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = newsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[1] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = newsFeeds[1];            
        }
        else if (newsFeeds.Count == 1)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.RedFontStart + newsSources[0] + gui.RedFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = newsFeeds[0];            
        }

        NewsRefreshRateLabel.Text = gui.GrayFontStart 
            + "...All News updated every " + newsRefreshRate.ToString() + " minutes." 
            + gui.GrayFontEnd;
    }

    private void RenderNews(StringCollection perStockNewsSources, StringCollection perStockNewsFeeds)
    {
        NewsMessageLabelA.Text = NewsMessageLabelB.Text = NewsMessageLabelC.Text = NewsMessageLabelD.Text = NewsMessageLabelE.Text = NewsMessageLabelF.Text = string.Empty;
        NewsLabelA.Text = NewsLabelB.Text = NewsLabelC.Text = NewsLabelD.Text = NewsLabelE.Text = NewsLabelF.Text = string.Empty;

        if (perStockNewsFeeds.Count == 4)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[0] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = perStockNewsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[1] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = perStockNewsFeeds[1];

            NewsMessageLabelC.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[2] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelC.Text = perStockNewsFeeds[2];

            NewsMessageLabelD.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[3] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelD.Text = perStockNewsFeeds[3];
        }
        else if (perStockNewsFeeds.Count == 3)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[0] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = perStockNewsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[1] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = perStockNewsFeeds[1];

            NewsMessageLabelC.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[2] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelC.Text = perStockNewsFeeds[2];           
        }
        else if (perStockNewsFeeds.Count == 2)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[0] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = perStockNewsFeeds[0];

            NewsMessageLabelB.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[1] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelB.Text = perStockNewsFeeds[1];            
        }
        else if (perStockNewsFeeds.Count == 1)
        {
            NewsMessageLabelA.Text = gui.BoldFontStart + gui.GreenFontStart + perStockNewsSources[0] + gui.GreenFontEnd + gui.BoldFontEnd;
            NewsLabelA.Text = perStockNewsFeeds[0];            
        }

        NewsRefreshRateLabel.Text = gui.GrayFontStart
            + "...All News updated every " + newsRefreshRate.ToString() + " minutes."
            + gui.GrayFontEnd;
    }    


    protected void NewsSearchButton_Click(object sender, EventArgs e)
    {
        NewsSearchMessageLabel.Text = "";
        string stockSymbol = NewsSearchTB.Text.Trim().ToUpper();
        
        if (!string.IsNullOrEmpty(stockSymbol))
        {   
            

            StringCollection perStockNewsFeeds = new StringCollection();
            StringCollection perStockNewsSources = new StringCollection();

            string newsFeed = "";
            try
            {
                string companyName = "";
                if (stockService.YahooCSVIsQuoteExists(stockSymbol, out companyName))
                {
                    foreach (DictionaryEntry de in perStockNewsFeedsTable)
                    {
                        string source = de.Key.ToString().Trim();
                        string feedURL = de.Value.ToString().Trim();

                        newsFeed = rssReader.LoadPerStockNews(feedURL, stockSymbol);

                        perStockNewsSources.Add(source);
                        perStockNewsFeeds.Add(newsFeed);
                    }
                    RenderNews(perStockNewsSources, perStockNewsFeeds);

                    LoadSymbolData(stockSymbol);

                }
                else
                {
                    NewsSearchMessageLabel.Text = gui.RedFontStart + " Please Enter a valid Symbol." + gui.RedFontEnd;
                }
            }
            catch (Exception ex)
            {
                NewsSearchMessageLabel.Text = gui.RedFontStart + " Please Try Again." + gui.RedFontEnd;
                log.Log("Exception in News.aspx for Symbol: " + stockSymbol + ": " + ex.Message);

            }
        }
        else
        {
            NewsSearchMessageLabel.Text = gui.RedFontStart + " Please Enter a valid Symbol." + gui.RedFontEnd;
        }
    }

    /// <summary>
    /// Load the Data related to the particular symbol: (Price, Change, Volume) 
    /// </summary>
    /// <param name="stockSymbol"></param>
    private void LoadSymbolData(string stockSymbol)
    {
        
    }

    protected void TopHeadlinesLink_Click(object sender, EventArgs e)
    {
        NewsSearchTB.Text = "";

        newsSources = new StringCollection();
        newsFeeds = new StringCollection();

        GetNews(newsFeedsTable);
        RenderNews();        
    }

    protected void IPOLink_Click(object sender, EventArgs e)
    {
        NewsSearchTB.Text = "";

        newsSources = new StringCollection();
        newsFeeds = new StringCollection();

        GetNews(ipoNewsFeedsTable);
        RenderNews();     
    }

    protected void GoBackLink_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(username))
        {
            Response.Redirect(links.LoginLink);
        }
        else
        {
            Response.Redirect(links.HomePageLink);
        }
    }
    protected void LogoutLink_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.LogoutPageLink);
    }
    
}
