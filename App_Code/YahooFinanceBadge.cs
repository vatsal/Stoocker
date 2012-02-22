using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for YahooFinanceBadge
/// </summary>
public class YahooFinanceBadge
{
    bool isShowYFB = Convert.ToInt32(ConfigurationManager.AppSettings["isShowYFB"]) == 1 ? true : false;

    static HtmlControl yahooFinanceBadgeIFrame = new System.Web.UI.HtmlControls.HtmlGenericControl("iframe");

    public bool IsShowYFB
    {
        get
        {
            return isShowYFB;
        }
    }

    public YahooFinanceBadge()
    {
        
    }

    /// <summary>
    /// Creates a Yahoo Finance Badge (YFB) that is embedded into different pages based on the requirements of that page.
    /// </summary>
    /// <param name="alreadySelectedStocks">The stocks to be listed in the YFB</param>
    /// <param name="badgeType"> (Quotes -> 1), (Charts/Quotes -> 2), (News/Charts/Quotes -> 3)</param>
    /// <param name="isYFBWhite">True -> YFB is White, False -> YFB is Black</param>
    /// <param name="frameborder">1 -> Yes FrameBorder, 0 -> No FrameBorder</param>
    /// <param name="scrolling">"Yes" -> Yes Scrolling is ON, "No" -> No Scrolling</param>
    /// <param name="widthInPixels">Width of the YFB (pixels)</param>
    /// <param name="heightInPixels">Height of the YFB (pixels)</param>
    /// <returns>An HTMLControl which is an IFrame and which can be embedded into any PlaceHolder Control</returns>
    public HtmlControl CreateYBF(string alreadySelectedStocks, int badgeType, bool isYFBWhite, int frameborder, string scrolling, int widthInPixels, int heightInPixels)
    {
        # region Initial Code Basic YFB
        //string src = "http://api.finance.yahoo.com/instrument/1.0/" + alreadySelectedStocks + "/badge;quote/HTML/f.white?AppID=iAr_Ye86oXBOsHJm.AROJqK1QbI-&sig=R8rw82HNwmHXV_988xa2GneZFes-&t=1192545599047";
                
        //HtmlControl yahooFinanceBadgeIFrame = new System.Web.UI.HtmlControls.HtmlGenericControl("iframe");
        //yahooFinanceBadgeIFrame.Attributes["src"] = src;
        //yahooFinanceBadgeIFrame.Attributes["frameborder"] = "0";
        //yahooFinanceBadgeIFrame.Attributes["scrolling"] = "no";

        //yahooFinanceBadgeIFrame.Attributes["allowtransparency"] = "true";
        //yahooFinanceBadgeIFrame.Attributes["hspace"] = "0";
        //yahooFinanceBadgeIFrame.Attributes["vspace"] = "0";
        //yahooFinanceBadgeIFrame.Attributes["marginwidth"] = "0";
        //yahooFinanceBadgeIFrame.Attributes["marginheight"] = "0";
        //yahooFinanceBadgeIFrame.Attributes["width"] = "200px";
        //yahooFinanceBadgeIFrame.Attributes["height"] = "400px";
        #endregion Initial Code Basic YFB
        
        //  "http://api.finance.yahoo.com/instrument/1.0/GOOG,YHOO/badge;quote/HTML?AppID=iAr_Ye86oXBOsHJm.AROJqK1QbI-&sig=R8rw82HNwmHXV_988xa2GneZFes-&t=1192545599047"
        //  "http://api.finance.yahoo.com/instrument/1.0/" + alreadySelectedStocks + "/badge;quote/HTML/f.white?AppID=iAr_Ye86oXBOsHJm.AROJqK1QbI-&sig=R8rw82HNwmHXV_988xa2GneZFes-&t=1192545599047";        
        //  "http://api.finance.yahoo.com/instrument/1.0/YHOO,GOOG/badge;chart=3m,,comparison;quote/HTML/f.white?AppID=LG6A8.86oXA7n4TGio5p01iIdSc-&sig=F708wGPoat92OmpVTX7w7i2wEnA-&t=1192633906671";
        string src = "http://api.finance.yahoo.com/instrument/1.0/" + alreadySelectedStocks + "/badge;";

        if (badgeType == 1)
        {
            src = src + "quote/HTML";
        }
        else if (badgeType == 2)
        {
            src = src + "chart=6m,,comparison;quote/HTML";
        }
        else if (badgeType == 3)
        {

        }

        if (isYFBWhite)
        {
            src = src + "/f.white";
        }
        
        if (badgeType == 1)
        {
            src = src + "?AppID=iAr_Ye86oXBOsHJm.AROJqK1QbI-&sig=R8rw82HNwmHXV_988xa2GneZFes-&t=1192545599047";
        }
        else if (badgeType == 2)
        {
            src = src + "?AppID=LG6A8.86oXA7n4TGio5p01iIdSc-&sig=F708wGPoat92OmpVTX7w7i2wEnA-&t=1192633906671";
        }
        else if (badgeType == 3)
        {
            //  Includes News/Charts/Quotes. Probably don't need it for now.
        }
        
        yahooFinanceBadgeIFrame.Attributes["src"] = src;
        yahooFinanceBadgeIFrame.Attributes["frameborder"] = frameborder.ToString();
        yahooFinanceBadgeIFrame.Attributes["scrolling"] = scrolling;
                
        yahooFinanceBadgeIFrame.Attributes["marginwidth"] = "0";
        yahooFinanceBadgeIFrame.Attributes["marginheight"] = "0";
        yahooFinanceBadgeIFrame.Attributes["width"] = widthInPixels.ToString() + "px";
        yahooFinanceBadgeIFrame.Attributes["height"] = heightInPixels.ToString() + "px";
        
        return yahooFinanceBadgeIFrame;
    }

    public string CreateYahooFinanceBadgeSrc(string alreadySelectedStocks, int badgeType, bool isYFBWhite)
    {
        string src = "http://api.finance.yahoo.com/instrument/1.0/" + alreadySelectedStocks + "/badge;";

        if (badgeType == 1)
        {
            src = src + "quote/HTML";
        }
        else if (badgeType == 2)
        {
            src = src + "chart=6m,,comparison;quote/HTML";
        }
        else if (badgeType == 3)
        {

        }

        if (isYFBWhite)
        {
            src = src + "/f.white";
        }

        if (badgeType == 1)
        {
            src = src + "?AppID=iAr_Ye86oXBOsHJm.AROJqK1QbI-&sig=R8rw82HNwmHXV_988xa2GneZFes-&t=1192545599047";
        }
        else if (badgeType == 2)
        {
            src = src + "?AppID=LG6A8.86oXA7n4TGio5p01iIdSc-&sig=F708wGPoat92OmpVTX7w7i2wEnA-&t=1192633906671";
        }
        else if (badgeType == 3)
        {
            //  Includes News/Charts/Quotes. Probably don't need it for now.
        }

        return src;
    }
}
