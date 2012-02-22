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

public partial class Stoocks : System.Web.UI.MasterPage
{
    Links links;
    GUIVariables gui;

    string copyrightText = ConfigurationManager.AppSettings["CopyrightText"];

    protected void Page_Load(object sender, EventArgs e)
    {
        links = (Links)Application["links"];

        gui = (GUIVariables)Application["gui"];

        CopyrightLabel.Text = gui.SmallFontStart + gui.GrayFontStart
            + copyrightText
            + gui.GrayFontEnd + gui.SmallFontEnd;
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
    protected void NewsLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.NewsPageLink);
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
    protected void FeedbackLinkButton_Click(object sender, EventArgs e)
    {
        Response.Redirect(links.FeedbackPageLink);
    }
    #endregion Footer Links
    
}
