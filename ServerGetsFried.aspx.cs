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

public partial class ServerGetsFried : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //  Destroy the Session.
        Session.Abandon();
        ServerGetsFriedLabel.Text = "Woops!! The Stoocky Little Server got Stoocked...Never mind, the mindless Vatsal is onto the Problem";
    }
}
