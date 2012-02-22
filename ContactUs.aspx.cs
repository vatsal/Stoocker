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

public partial class ContactUs : System.Web.UI.Page
{
    GUIVariables gui;

    protected void Page_Load(object sender, EventArgs e)
    {
        gui = (GUIVariables)Application["gui"];

        ContactLabel.Text = "For any and all inquiries, contact " 
            + gui.RedFontStart + "Vatsal H. Shah" + gui.RedFontEnd + " " 
            + gui.BoldFontStart + gui.GrayFontStart + "(E-Mail: com dot vatsals at vatsals)" + gui.GrayFontEnd + gui.BoldFontEnd;
        
    }
}
