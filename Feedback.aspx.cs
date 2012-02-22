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

public partial class Feedback : System.Web.UI.Page
{
    General general;
    Logger log;
    GUIVariables gui;

    string stoockerMail = ConfigurationManager.AppSettings["stoockerMail"];

    protected void Page_Load(object sender, EventArgs e)
    {
        general = (General)Application["general"];
        log = (Logger)Application["log"];
        gui = (GUIVariables)Application["gui"];

        MessageLabel.Text = "Your Feedback is so very important that " + gui.StoocksFont + " had to dedicate an entire page for it!";
    }
    
    protected void SubmitFeedbackButton_Click(object sender, EventArgs e)
    {
        string name = NameTB.Text.Trim();
        string eMail = EMailTB.Text.Trim();
        string topic = TopicDDL.SelectedItem.Value;
        string feedback = FeedbackTB.Text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(eMail) || string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(feedback))
        {
            OutputLabel.Text = "Please fill out all the fields.";
        }
        else
        {
            if (general.IsValidEMail(eMail))
            {
                log.LogFeedback(name, eMail, topic, feedback);                
                OutputLabel.Text = "Thank You for helping us improve " + gui.StoocksFont;                
            }
        }
    }

}
