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

public partial class SessionExpired : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //  Destroy the Session.
        //  Session.Abandon();

        HttpCookie stoockerCookie = Request.Cookies["stoockerCookie"];
        if (stoockerCookie != null)
        {
            stoockerCookie.Expires = DateTime.Now.AddMinutes(-1);
            Response.Cookies.Add(stoockerCookie);
        }

        SessionExpiredLabel.Text = "Woops!! Your Stoocker Session Expired. Redirecting you back to the Login Page...";
    }
}
