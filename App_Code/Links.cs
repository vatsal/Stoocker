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
/// Summary description for Links
/// </summary>
public class Links
{
    string _loginLink = ConfigurationManager.AppSettings["loginLink"];

    string _homePageLink = ConfigurationManager.AppSettings["homePageLink"];
    string _interestsPageLink = ConfigurationManager.AppSettings["interestsPageLink"];    
    string _userPredictionHistoryPageLink = ConfigurationManager.AppSettings["userPredictionHistoryPageLink"];
    string _newsPageLink = ConfigurationManager.AppSettings["newsPageLink"];
    
    string _logoutPageLink = ConfigurationManager.AppSettings["logoutPageLink"];
    string _sessionExpiredPageLink = ConfigurationManager.AppSettings["sessionExpiredPageLink"];
    string _errorPageLink = ConfigurationManager.AppSettings["errorPageLink"];

    string _aboutPageLink = ConfigurationManager.AppSettings["aboutPageLink"];
    string _contactUsPageLink = ConfigurationManager.AppSettings["contactUsPageLink"];
    string _privacyPolicyPageLink = ConfigurationManager.AppSettings["privacyPolicyPageLink"];
    string _feedbackPageLink = ConfigurationManager.AppSettings["feedbackPageLink"];

    

    public Links()
    {

    }

    public string LoginLink
    {
        get { return _loginLink; }
    }

    public string HomePageLink
    {
        get { return _homePageLink; }
    }
    public string InterestsPageLink
    {
        get { return _interestsPageLink; }
    }
    public string UserPredictionHistoryPageLink
    {
        get { return _userPredictionHistoryPageLink; }
    }
    public string NewsPageLink
    {
        get { return _newsPageLink; }
    }
    public string LogoutPageLink
    {
        get { return _logoutPageLink; }
    }
    public string SessionExpiredPageLink
    {
        get { return _sessionExpiredPageLink; }
    }
    public string ErrorPageLink
    {
        get { return _errorPageLink; }
    }

    public string AboutPageLink
    {
        get { return _aboutPageLink; }
    }
    public string ContactUsPageLink
    {
        get { return _contactUsPageLink; }
    }
    public string PrivacyPolicyPageLink
    {
        get { return _privacyPolicyPageLink; }
    }
    public string FeedbackPageLink
    {
        get { return _feedbackPageLink; }
    }

}
