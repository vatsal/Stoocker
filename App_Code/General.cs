using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

/// <summary>
/// Summary description for General
/// </summary>
public class General
{    
    Logger log = new Logger();    

    private string smtpClient = ConfigurationManager.AppSettings["smtpClient"];

    public General()
    {
        
    }

    public bool SendMail(string fromAddress, string toAddress, string subject, string userMessage)
    {
        try
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            System.DateTime currentTime = System.DateTime.Now;

            message.IsBodyHtml = true;
            message.Subject = subject;
            message.To.Add(toAddress);
            message.From = new System.Net.Mail.MailAddress(fromAddress);
            message.Body = userMessage
                            + @"<br/>"
                            + @"<br/>"
                            + @"<br/>"
                            + "Visit Stoocker.com"
                            + @"<br/>";


            //  System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail.hakia.com");
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(smtpClient);
            smtp.Send(message);
            return true;
        }
        catch (Exception ex)
        {
            if (log.isLoggingOn && log.isAppLoggingOn)
            {
                log.Log(ex);
            }
            return false;
        }
    }

    public bool IsValidEMail(string email)
    {
        string emailPattern = @"^(([^<>()[\]\\.,;:\s@\""]+"
                                    + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                                    + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                                    + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                                    + @"[a-zA-Z]{2,}))$";
        Regex emailRegex = new Regex(emailPattern, RegexOptions.Compiled);
        return emailRegex.IsMatch(email);
    }

    public bool IsAlphabetOrNumber(string input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            if (!char.IsLetterOrDigit(input[i]))
            {
                return false;
            }
        }
        return true;
    }

    


}
