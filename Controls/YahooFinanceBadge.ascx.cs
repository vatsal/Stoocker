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

public partial class Controls_YahooFinanceBadge : System.Web.UI.UserControl
{
    protected String yahooChart = String.Empty;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            UserControl YahooFinanceBadgeIFrame = (UserControl)Page.FindControl("YahooFinanceBadgeIFrame");
            
            YahooFinanceBadge yfb = new YahooFinanceBadge();
            string src = yfb.CreateYahooFinanceBadgeSrc("YHOO,MSFT", 2, true);
            YahooFinanceBadgeIFrame.Attributes.Add("src", src);
            
        }
        catch (Exception ex)
        {
            yahooChart = "No Data Available." + " Exception: " + ex.Message;
        }
    }
}
