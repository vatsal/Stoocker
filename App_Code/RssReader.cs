using System;
using System.Xml;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Caching;
using System.Configuration;

/// <summary>
/// Parses remote RSS 2.0 feeds.
/// </summary>
[Serializable]
public class RssReader : IDisposable
{
    bool isShowNews = Convert.ToInt32(ConfigurationManager.AppSettings["isShowNews"]) == 0 ? false : true;
    int maxNewsItemsPerUser = Convert.ToInt32(ConfigurationManager.AppSettings["maxNewsItemsPerUser"]);
    string yahooGenericNewsFeedRSSFinanceURL = ConfigurationManager.AppSettings["YahooGenericNewsFeedRSSFinanceURL"];
    string yahooNewsFeedRSSFinanceURL = ConfigurationManager.AppSettings["YahooNewsFeedRSSFinanceURL"];
    int newsRefreshRate = Convert.ToInt32(ConfigurationManager.AppSettings["newsRefreshRate"]);

    Logger log;


    #region Constructors

    public RssReader()
    { 
        log = new Logger();
    }

    public RssReader(string feedUrl)
    {
        _FeedUrl = feedUrl;
    }

    #endregion

    #region Properties

    private string _FeedUrl;
    private Collection<RssItem> _Items = new Collection<RssItem>();
    private string _Title;
    private string _Description;
    private TimeSpan _UpdateFrequency;

    /// <summary>
    /// Gets or sets the URL of the RSS feed to parse.
    /// </summary>
    public string FeedUrl
    {
        get 
        { 
            return _FeedUrl; 
        }
        set
        {
            _FeedUrl = value; 
        }
    }
    
    /// <summary>
    /// Gets all the items in the RSS feed.
    /// </summary>
    public Collection<RssItem> Items
    {
        get 
        {
            return _Items; 
        }
    }
        
    /// <summary>
    /// Gets the title of the RSS feed.
    /// </summary>
    public string Title
    {
        get 
        { 
            return _Title; 
        }
    }
        
    /// <summary>
    /// Gets the description of the RSS feed.
    /// </summary>
    public string Description
    {
        get 
        { 
            return _Description; 
        }
    }

    private DateTime _LastUpdated;
    /// <summary>
    /// Gets the date and time of the retrievel and
    /// parsing of the remote RSS feed.
    /// </summary>
    public DateTime LastUpdated
    {
        get 
        {
            return _LastUpdated; 
        }
    }
        
    /// <summary>
    /// Gets the time before the feed get's silently updated.
    /// Is TimeSpan.Zero unless the CreateAndCache method has been used.
    /// </summary>
    public TimeSpan UpdateFrequency
    {
        get 
        { 
            return _UpdateFrequency; 
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an RssReader instance from the specified URL and inserts it into the cache. 
    /// When it expires from the cache, it automatically retrieves the remote RSS feed and inserts it to the cache again.
    /// </summary>
    /// <param name="feedUrl">The URI of the RSS feed.</param>
    /// <param name="updateFrequency">The time before it should update it self.</param>
    /// <returns>An instance of the RssReader class.</returns>
    public static RssReader CreateAndCache(string feedUrl, TimeSpan updateFrequency)
    {
        if (HttpRuntime.Cache["RssReader_" + feedUrl] == null)
        {
          RssReader reader = new RssReader(feedUrl);
          reader.Execute();
          reader._UpdateFrequency = updateFrequency;
          HttpRuntime.Cache.Add("RssReader_" + feedUrl, reader, null, DateTime.Now.Add(updateFrequency), Cache.NoSlidingExpiration, CacheItemPriority.Normal, RefreshCache);
        }

        return (RssReader)HttpContext.Current.Cache["RssReader_" + feedUrl];
    }

    /// <summary>
    /// Retrieves the remote RSS feed and inserts it into the cache
    /// when it has expired.
    /// </summary>
    private static void RefreshCache(string key, object item, CacheItemRemovedReason reason)
    {
        if (reason != CacheItemRemovedReason.Removed)
        {
            string feedUrl = key.Replace("RssReader_", String.Empty);
            RssReader reader = new RssReader(feedUrl);
            reader.Execute();
            reader._UpdateFrequency = ((RssReader)item).UpdateFrequency;
            HttpRuntime.Cache.Add("RssReader_" + feedUrl, reader, null, DateTime.Now.Add(reader.UpdateFrequency), Cache.NoSlidingExpiration, CacheItemPriority.Normal, RefreshCache);
        }
    }

    /// <summary>
    /// Retrieves the remote RSS feed and parses it.
    /// </summary>
    /// <exception cref="System.Net.WebException" />
    public Collection<RssItem> Execute()
    {        
        if (String.IsNullOrEmpty(FeedUrl))
            throw new ArgumentException("The feed url must be set");
        try
        {

            using (XmlReader reader = XmlReader.Create(FeedUrl))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                ParseElement(doc.SelectSingleNode("//channel"), "title", ref _Title);
                ParseElement(doc.SelectSingleNode("//channel"), "description", ref _Description);
                ParseItems(doc);

                _LastUpdated = DateTime.Now;

                return _Items;
            }
        }
        catch(Exception ex)
        {
            if (log.isAppLoggingOn)
            {
                log.Log(ex);
            }

            RssItem item = new RssItem();
            item.Link = "";
            item.Date = DateTime.Now;
            item.Title = "";
            item.Description = "";
            _Items.Add(item);
            return _Items;
        }
    }

    /// <summary>
    /// Parses the xml document in order to retrieve the RSS items.
    /// </summary>
    private void ParseItems(XmlDocument doc)
    {
        _Items.Clear();
        XmlNodeList nodes = doc.SelectNodes("rss/channel/item");

        foreach (XmlNode node in nodes)
        {
           
                RssItem item = new RssItem();
                ParseElement(node, "title", ref item.Title);
                ParseElement(node, "description", ref item.Description);
                ParseElement(node, "link", ref item.Link);

                string date = null;
                ParseElement(node, "pubDate", ref date);
                DateTime.TryParse(date, out item.Date);

                if (_Items.Count < maxNewsItemsPerUser)
                {
                    _Items.Add(item);
                }
                else
                {
                    break;
                }            
        }
    }

    /// <summary>
    /// Parses the XmlNode with the specified XPath query 
    /// and assigns the value to the property parameter.
    /// </summary>
    private void ParseElement(XmlNode parent, string xPath, ref string property)
    {
        try
        {
            XmlNode node = parent.SelectSingleNode(xPath);
            if (node != null)
                property = node.InnerText;
            else
                property = "Unresolvable";
        }
        catch
        {

        }
    }

    /// <summary>
    /// Get News Feeds from the assigned RSS Aggregator.
    /// </summary>
    /// <param name="alreadySelectedStocks">The Symbols of User selected Stocks, in CSV format.</param>
    /// <returns>A string containing (News Title, News Link)</returns>
    public string LoadPerStockNews(string alreadySelectedStocks)
    {
        string newsFeeds = "";

        if (isShowNews)
        {
            GUIVariables gui = new GUIVariables();

            //    //    You can use the class with Caching.
            //    RssReader reader = RssReader.CreateAndCache("http://feeds.feedburner.com/netslave", new TimeSpan(2, 0, 0));
            //    foreach (RssItem item in reader.Items)
            //    {
            //        Response.Write(item.Title +"<br />");
            //    }
            
            //  You can also use the class directly without caching.
            using (RssReader rss = new RssReader())
            {                   
                //  Show personalized News for all Users, Don't Cache.                
                rss.FeedUrl = yahooNewsFeedRSSFinanceURL + alreadySelectedStocks;            

                foreach (RssItem item in rss.Execute())
                {
                    //  string makeLink = item.Title + "    " + "[" + "<a href='" + item.Link + "' target='_blank'>" + "link" + "</a>" + "]";
                    //  newsFeeds = newsFeeds + makeLink + "    " + gui.LineBreak;

                    string makeLink = gui.RedFontStart + ">>" + gui.RedFontEnd 
                        + "<a href='" + item.Link + "' style=\"text-decoration:none; font-family:Verdana;\" target='_blank'>" + item.Title + "</a>";
                    newsFeeds = newsFeeds + makeLink + "    " + gui.LineBreak;
                }
            }
        }
        return newsFeeds;
    }


    /// <summary>
    /// Get News Feeds from the assigned RSS Aggregator.
    /// </summary>
    /// <param name="feedURL">URL to fetch the news feed.</param>
    /// <param name="alreadySelectedStocks">The Symbols of User selected Stocks, in CSV format.</param>
    /// <returns>A string containing (News Title, News Link)</returns>
    public string LoadPerStockNews(string feedURL, string alreadySelectedStocks)
    {
        string newsFeeds = "";

        if (isShowNews)
        {
            GUIVariables gui = new GUIVariables();           
            using (RssReader rss = new RssReader())
            {                
                rss.FeedUrl = feedURL + alreadySelectedStocks;

                foreach (RssItem item in rss.Execute())
                {
                    //  string makeLink = item.Title + "    " + "[" + "<a href='" + item.Link + "' target='_blank'>" + "link" + "</a>" + "]";
                    //  newsFeeds = newsFeeds + makeLink + "    " + gui.LineBreak;

                    string makeLink = gui.RedFontStart + ">>" + gui.RedFontEnd 
                        + "<a href='" + item.Link + "' style=\"text-decoration:none; font-family:Verdana;\" target='_blank'>" + item.Title + "</a>";
                    newsFeeds = newsFeeds + makeLink + "    " + gui.LineBreak;
                }
            }
        }
        return newsFeeds;
    }


    public string LoadGenericNews(string url, bool isByPassCache)
    {
        string newsFeeds = "";

        if (isShowNews)
        {
            GUIVariables gui = new GUIVariables();
            if (!isByPassCache && HttpRuntime.Cache["genericNews"] != null)
            {
                newsFeeds = Convert.ToString(HttpRuntime.Cache["genericNews"]);
            }
            else
            {
                using (RssReader rss = new RssReader())
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        rss.FeedUrl = yahooGenericNewsFeedRSSFinanceURL;
                    }
                    else
                    {
                        rss.FeedUrl = url;
                    }

                    foreach (RssItem item in rss.Execute())
                    {
                        //  string makeLink = item.Title + "    " + "[" + "<a href='" + item.Link + "' target='_blank'>" + "link" + "</a>" + "]";
                        //  newsFeeds = newsFeeds + makeLink + "    " + gui.LineBreak;

                        string makeLink = gui.RedFontStart + ">>" + gui.RedFontEnd 
                            + "<a href='" + item.Link + "' style=\"text-decoration:none; font-family:Verdana;\" target='_blank'>" + item.Title + "</a>";
                        newsFeeds = newsFeeds + makeLink + "    " + gui.LineBreak;

                    }
                    //  HttpRuntime.Cache.Insert("genericNews", newsFeeds, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(newsRefreshRate));
                    HttpRuntime.Cache.Insert("genericNews", newsFeeds, null, DateTime.Now.AddMinutes(newsRefreshRate), System.Web.Caching.Cache.NoSlidingExpiration);                    
                }
            }            
        }
        return newsFeeds;
    }


    #endregion

    #region IDisposable Members

    private bool _IsDisposed;

    /// <summary>
    /// Performs the disposal.
    /// </summary>
    private void Dispose(bool disposing)
    {
        if (disposing && !_IsDisposed)
        {
            _Items.Clear();
            _FeedUrl = null;
            _Title = null;
            _Description = null;
        }

        _IsDisposed = true;
    }

    /// <summary>
    /// Releases the object to the garbage collector
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

}

#region RssItem struct

/// <summary>
/// Represents a RSS feed item.
/// </summary>
[Serializable]
public struct RssItem
{
    /// <summary>
    /// The publishing date.
    /// </summary>
    public DateTime Date;

    /// <summary>
    /// The title of the item.
    /// </summary>
    public string Title;

    /// <summary>
    /// A description of the content or the content itself.
    /// </summary>
    public string Description;

    /// <summary>
    /// The link to the webpage where the item was published.
    /// </summary>
    public string Link;
}

#endregion