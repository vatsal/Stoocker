using System;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;

using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;

using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Web.Caching;

/// <summary>
/// Summary description for Stoocker StockService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class StockService : System.Web.Services.WebService
{
    private string yahooFinanceURL = System.Configuration.ConfigurationManager.AppSettings["YahooFinanceURL"];
    private string xIgniteFinanceURL = System.Configuration.ConfigurationManager.AppSettings["XIgniteFinanceURL"];
    private string yahooCSVFinanceURL = System.Configuration.ConfigurationManager.AppSettings["YahooCSVFinanceURL"];

    Logger log = new Logger();
    
    public StockService()
    {
            
    }

    /// <summary>
    /// Used by both 'Yahoo Quote Scrape Methods' and 'XIgnite Quote Scrape Methods' for scraping the web-page.
    /// </summary>
    /// <param name="url">The URL of the Web-Page</param>
    /// <returns>The contents of the Web-Page</returns>
    private string GetPageContent(string url)
    {
        WebRequest wReq;
        WebResponse wRes;
        StreamReader sr;
        String content;

        wReq = HttpWebRequest.Create(url);
        wRes = wReq.GetResponse();
        sr = new StreamReader(wRes.GetResponseStream());
        content = sr.ReadToEnd();
        sr.Close();

        return content;
    }


    #region Yahoo Quote Scrape Methods

    [WebMethod]
    public string YahooGetQuote(string ticker)
    {
        string stockURL, page, retval;
        try
        {
            stockURL = YahooGetURL(ticker);
            page = GetPageContent(stockURL);
            retval = YahooScrapeQuotePriceFromPage(page);
        }
        catch (ArgumentOutOfRangeException)
        {
            retval = "Invalid Ticker!";
        }
        catch (Exception)
        {
            retval = "Unknown Error";
        }
        return retval;
    }

    [WebMethod]
    public DataSet YahooGetQuotes(string tickers)
    {
        char[] splitter = { ' ' };
        string[] _tickers = tickers.Trim().Split(splitter);
        Int32 i, ticks;

        ticks = _tickers.Length;

        DataSet ds = new DataSet();
        DataTable dt = new DataTable("StockData");
        DataColumn dc;

        dc = dt.Columns.Add("Ticker", System.Type.GetType("System.String"));
        dc = dt.Columns.Add("Price", System.Type.GetType("System.String"));

        for (i = 0; i < ticks; i++)
        {
            DataRow dr = dt.NewRow();
            dr["Ticker"] = _tickers[i].ToUpper();
            dr["Price"] = YahooGetQuote(_tickers[i]);
            dt.Rows.Add(dr);
        }

        ds.Tables.Add(dt);
        return ds;
    }

    private string YahooGetURL(string ticker)
    {
        StringBuilder url = new StringBuilder();

        url.Append(yahooFinanceURL);
        url.Append(ticker);

        return url.ToString();
    }

    private string YahooScrapeQuotePriceFromPage(string page)
    {
        Int32 i;

        i = page.IndexOf("Last Trade:");
        page = page.Substring(i);

        i = page.IndexOf("<b>");
        page = page.Substring(i);

        i = page.IndexOf("</b>");
        page = page.Substring(0, i);

        page = Regex.Replace(page, "<b>", "");
        return page;
    }

    #endregion Yahoo Quote Scrape Methods

    #region XIgnite Quote Scrape Methods

    [WebMethod]
    public void XIgniteGetQuoteData(string ticker, out string currentPrice, out string change, out string previousClose)
    {
        string stockURL, page;
        try
        {
            stockURL = XIgniteGetURL(ticker);
            page = GetPageContent(stockURL);

            currentPrice = XIgniteScrapeTagFromXMLPage(page, "Last");
            change = XIgniteScrapeTagFromXMLPage(page, "Change");
            previousClose = XIgniteScrapeTagFromXMLPage(page, "Previous_Close");            
        }
        catch (ArgumentOutOfRangeException)
        {
            currentPrice = "Invalid Ticker";
            change = "Invalid Ticker";
            previousClose = "Invalid Ticker";
        }
        catch (Exception)
        {
            currentPrice = "Unknown Error";
            change = "Unknown Error";
            previousClose = "Unknown Error";
        }        
    }

    
    private string XIgniteGetURL(string ticker)
    {
        StringBuilder url = new StringBuilder();

        url.Append(xIgniteFinanceURL);
        url.Append(ticker);

        return url.ToString();
    }

    

    private string XIgniteScrapeTagFromXMLPage(string page, string xmlTag)
    {
        string startTag = "<" + xmlTag + ">";
        string endTag = "</" + xmlTag + ">";

        Int32 i, j;

        i = page.IndexOf(startTag);
        page = page.Substring(i);

        j = page.IndexOf(endTag);
        page = page.Substring(0, j);

        page = page.Replace(startTag, "");        

        return page;
    }


    public bool XIgniteIsQuoteExists(string ticker)
    {
        string symbolNotFoundMessage = "Symbol not found.";
        string stockURL, page;
        
        bool isExists = false;        
        
        try
        {
            stockURL = XIgniteGetURL(ticker);
            page = GetPageContent(stockURL);

            string message = XIgniteScrapeTagFromXMLPage(page, "Message");
            if (message.Equals(symbolNotFoundMessage))
            {
                isExists = false; 
            }
            else
            {
                isExists = true; 
            }            
        }
        catch (ArgumentOutOfRangeException)
        {
            isExists = false;
        }
        catch (Exception)
        {
            isExists = false;
        }        
        return isExists;
    }

    public bool XIgniteIsQuoteExists(string ticker, out string companyName)
    {
        string symbolNotFoundMessage = "Symbol not found.";
        string stockURL, page;

        bool isExists = false;

        try
        {
            stockURL = XIgniteGetURL(ticker);
            page = GetPageContent(stockURL);

            string message = XIgniteScrapeTagFromXMLPage(page, "Message");
            if (message.Equals(symbolNotFoundMessage))
            {
                isExists = false;                
                companyName = "";
            }
            else
            {
                isExists = true;
                companyName = XIgniteScrapeTagFromXMLPage(page, "Name");
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            isExists = false;
            companyName = "";
        }
        catch (Exception)
        {
            isExists = false;
            companyName = "";
        }
        return isExists;
    }


    #region Methods Not Being Used Currently

    // Delete Start //   

    //private string XIgniteScrapeQuotePriceFromPage(string page)
    //{
    //    Int32 i, j;

    //    i = page.IndexOf("<Last>");
    //    page = page.Substring(i);

    //    j = page.IndexOf("</Last>");
    //    page = page.Substring(6,j);
        
    //    return page;
    //}

    //private string XIgniteScrapePercentChangeFromPage(string page)
    //{
    //    Int32 i, j;

    //    i = page.IndexOf("<Change>");
    //    page = page.Substring(i);

    //    j = page.IndexOf("</Change>");
    //    page = page.Substring(8, j);

    //    return page;
    //}

    //private string XIgniteScrapePreviousCloseFromPage(string page)
    //{
    //    Int32 i, j;

    //    i = page.IndexOf("<Previous_Close>");
    //    page = page.Substring(i);

    //    j = page.IndexOf("</Previous_Close>");
    //    page = page.Substring(16, j);

    //    return page;
    //}

    //  Works for Price, Change and Previous Close but was not working for Message Tag.
    //  Message Tag was required for finding if a stock symbol exists or not.
    //private string XIgniteScrapeTagFromXMLPage(string page, string xmlTag)
    //{       
    //    string startTag = "<" + xmlTag + ">";
    //    string endTag = "</" + xmlTag + ">";

    //    Int32 i, j;

    //    i = page.IndexOf(startTag);
    //    page = page.Substring(i);

    //    j = page.IndexOf(endTag);
    //    page = page.Substring(startTag.Length, j);

    //    return page;
    //}

    // Delete End // 

    #endregion Methods Not Being Used Currently


    #endregion XIgnite Quote Scrape Methods

    #region Yahoo Quote CSV Methods 
        
    /// <summary>
    /// Extract the Value for the corresponding Symbol/Stat Combination from the CSV.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="stat"></param>
    /// <returns>A single output string containing the value for the corresponding Symbol/Stat</returns>
    private string YahooCSVPage(string symbol, string stat)
    {
        WebClient wc = new WebClient();
        string url = yahooCSVFinanceURL + symbol + "&f=" + stat;
        string output = wc.DownloadString(url);

        string[] outputValues = output.Split(',');
        return outputValues[0].Trim();
    }


    /// <summary>
    /// Extract Multiple Values for the corresponding Symbol(s)/Stat(s) Combination from the CSV.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="stat"></param>
    /// <returns>A string containing multiple values for the corresponding Symbol(s)/Stat(s)</returns>    
    public string YahooCSVPageMatrix(string symbols, string stats)
    {
        WebClient wc = new WebClient();
        string url = yahooCSVFinanceURL + symbols + "&f=" + stats;
        string output = wc.DownloadString(url);

        return output;
        
    }

    
    public void YahooCSVGetQuoteData(string ticker, out string currentPrice, out string change, out string previousClose)
    {        
        if (!string.IsNullOrEmpty(ticker))
        {
            try
            {
                currentPrice = YahooCSVPage(ticker, "l1");
                change = YahooCSVPage(ticker, "c1");

                double currentPriceDouble;
                double changeDouble;
                double previousCloseDouble = 0;

                bool isCurrentPriceDouble = double.TryParse(currentPrice, out currentPriceDouble);
                bool isChangeDouble = double.TryParse(change, out changeDouble);

                if (isCurrentPriceDouble && isChangeDouble)
                {
                    previousCloseDouble = currentPriceDouble - changeDouble;

                    previousClose = Convert.ToString(previousCloseDouble);
                }
                else
                {
                    currentPrice = "Error";
                    change = "Error";
                    previousClose = "Error";
                }

                
            }
            catch (Exception ex)
            {
                currentPrice = "Error";
                change = "Error";
                previousClose = "Error";

                if (log.isLoggingOn && log.isAppLoggingOn)
                {
                    log.Log(ex);
                }
            }
        }
        else
        {
            currentPrice = "Invalid Ticker";
            change = "Invalid Ticker";
            previousClose = "Invalid Ticker";
        }
    }

    public bool YahooCSVIsQuoteExists(string ticker, out string companyName)
    {
        //  string symbolNotFoundMessage = "Missing Symbols List.";        
        bool isExists = false;

        try
        {
            companyName = YahooCSVPage(ticker, "nst1").ToUpper();
            companyName = companyName.Replace("\"", "").Replace("'", "");            

            isExists = true;
        }        
        catch (Exception ex)
        {            
            companyName = "";
            isExists = false;

            if (log.isLoggingOn && log.isAppLoggingOn)
            {
                log.Log(ex);
            }
        }
        return isExists;
    }

    //  YahooCSVGetQuotesMatrix (Method (A)) might be an optimization over YahooCSVGetQuoteData (Method (B)).
    //  Since A calls on Yahoo CSV Website only once per user session, 
    //  While B calls on Yahoo CSV Website n*m times per user session, where n=Stocks in User's Portfolio and m=Parameters to be retrieved (Price, Change, etc.)
    //  Although this will require some change on the caller's side, since A returns a StringCollection which now has to be processed on the caller's side.

    /// <summary>
    /// Return all the data required in a Tabular Format.
    /// </summary>
    /// <param name="tickers"></param>
    /// <returns></returns>
    [WebMethod]
    public DataSet YahooCSVGetQuotesMatrix(string tickers)
    {
        string quotesDataStr = "";
        DataSet ds = new DataSet();
        
        if (!string.IsNullOrEmpty(tickers))
        {
            
            try
            {
                string[] valueSplitter = { "," };
                string[] ticks = tickers.Trim().Split(valueSplitter, StringSplitOptions.RemoveEmptyEntries);
                
                //  Retrieves Price=l1 and Change=c1;                          
                quotesDataStr  = YahooCSVPageMatrix(tickers, "l1c1");

                //  string[] stockSplitter = { "\r\n" };
                //  Instead of \r\n, sometimes CSV contains only \n
                //  Thus a better way is to convert \r\n (or \n) to '|'
                quotesDataStr = quotesDataStr.Replace("\r\n", "|");
                quotesDataStr = quotesDataStr.Replace("\n", "|");
                string[] stockSplitter = { "|" };
                
                string[] price_change = quotesDataStr.Split(stockSplitter, StringSplitOptions.RemoveEmptyEntries);
                //  30.68,+0.04\r\n675.82,+0.05\r\n"                                
                
                
                DataTable dt = new DataTable("StockData");
                DataColumn dc;

                dc = dt.Columns.Add("Stock", System.Type.GetType("System.String"));
                dc = dt.Columns.Add("Price", System.Type.GetType("System.String"));
                dc = dt.Columns.Add("Change", System.Type.GetType("System.String"));
                dc = dt.Columns.Add("PreviousClose", System.Type.GetType("System.String"));
                
                for (int i = 0; i < ticks.Length; i++)
                {
                    DataRow dr = dt.NewRow();
                   
                    string[] values = price_change[i].Split(valueSplitter, StringSplitOptions.RemoveEmptyEntries);
                    double price  = Convert.ToDouble(values[0]);                    
                    double change = Convert.ToDouble(values[1]);

                    dr["Stock"] = ticks[i].ToUpper();
                    dr["Price"] = values[0];
                    dr["Change"] = values[1];                    
                    dr["PreviousClose"] = Convert.ToString(price - change);
                    
                    dt.Rows.Add(dr);
                }
                
                ds.Tables.Add(dt);
                return ds;
            }
            catch (Exception ex)
            {                
                ds = null;
                if (log.isLoggingOn && log.isAppLoggingOn)
                {
                    log.Log(ex);
                }
            }
        }
        else
        {
            ds = null;
        }
        return ds;
    }

    public string GetTimeOfLastTrade(string ticker)
    {
        string dateTimeOfLastTrade = "";
        try
        {
            dateTimeOfLastTrade = YahooCSVPageMatrix(ticker, "d1t1").Replace("\"", "").ToUpper();
        }
        catch (Exception ex)
        {
            log.Log(ex);
        }
        return dateTimeOfLastTrade;
    }


    #endregion Yahoo Quote CSV Methods


  
}

