using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.IO;

/// <summary>
/// Summary description for Logger
/// </summary>
public class Logger
{
    public bool isLoggingOn = Convert.ToInt32(ConfigurationManager.AppSettings["isLoggingOn"]) != 0 ? true : false;
    public bool isAppLoggingOn = Convert.ToInt32(ConfigurationManager.AppSettings["isAppLoggingOn"]) != 0 ? true : false;
    public bool isDBLoggingOn = Convert.ToInt32(ConfigurationManager.AppSettings["isDBLoggingOn"]) != 0 ? true : false;
    
    string logPath = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["logPath"];
    string feedbackPath = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["feedbackPath"];
    
    private string dateFormatString = ConfigurationManager.AppSettings["dateFormatString"];
    
    string today;
    string time;

    //  string tab = "  ";
    //  string space = " ";
    string rowSeperator = "####";
    string colSeperator = " | ";

	public Logger()
	{       
    
	}

    /// <summary>
    /// Log the Exception that occured inside the Application.
    /// </summary>
    /// <param name="ex">Exception</param>
    /// <returns>IF Logging Successful THEN True ELSE False</returns>
    public void Log(Exception ex)
    {
        if (isLoggingOn)
        {
            try
            {
                today = DateTime.Now.ToString(dateFormatString);
                time = DateTime.Now.ToString("hh:mm:ss");

                string fileName = today;
                
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                string path = Path.Combine(logPath, fileName);

                string exceptionString = rowSeperator + Environment.NewLine
                    + time + colSeperator + "Type: Exception" + colSeperator + ex.Message + Environment.NewLine
                    + "Stack Trace: " + Environment.NewLine + ex.StackTrace + Environment.NewLine;

                File.AppendAllText(path, exceptionString);
            }
            catch (Exception logEx)
            {
                //  Log(logEx);
            }
        }        
    }

    /// <summary>
    /// Log the Information.
    /// </summary>
    /// <param name="ex">Information To Log</param>
    /// <returns>IF Logging Successful THEN True ELSE False</returns>    
    public void Log(string information)
    {
        try
        {
            if (isLoggingOn)
            {
                today = DateTime.Now.ToString(dateFormatString);
                time = DateTime.Now.ToString("hh:mm:ss");

                string fileName = today;
                
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                
                string path = Path.Combine(logPath, fileName);

                string exceptionString = rowSeperator + Environment.NewLine
                    + time + colSeperator + "Type: Information" + colSeperator + information + Environment.NewLine;

                File.AppendAllText(path, exceptionString);
            }
        }
        catch (Exception logEx)
        {
            //  Log(logEx);
        }
    }

    public void LogFeedback(string name, string eMail, string topic, string feedback)
    {
        try
        {            
            today = DateTime.Now.ToString(dateFormatString);
            time = DateTime.Now.ToString("hh:mm:ss");

            string fileName = "Feedback";

            if (!Directory.Exists(feedbackPath))
            {
                Directory.CreateDirectory(feedbackPath);
            }

            string path = Path.Combine(feedbackPath, fileName);

            string feedbackString = rowSeperator + Environment.NewLine
                + time + colSeperator + name + colSeperator + eMail + colSeperator + topic + colSeperator + feedback 
                + Environment.NewLine;

            File.AppendAllText(path, feedbackString);
     
        }
        catch (Exception logEx)
        {
            //  Log(logEx);
            
        }
    }

}
