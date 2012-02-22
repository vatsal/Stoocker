using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Security.Cryptography;

using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Security;
using System.IO;

/// <summary>
/// DBOperations encapsulates all the DB related Operations.
/// </summary>
[assembly: AllowPartiallyTrustedCallers()]
public class DBOperations
{
    Logger log = new Logger();
    private string connStr = ConfigurationManager.ConnectionStrings["connStr"].ToString();

    static byte[] cryptBytes = System.Text.ASCIIEncoding.ASCII.GetBytes("Stoocker");

    public DBOperations()
    {
        
    }
    
    public string HashPassword(string password)
    {
        //string hashPassword = "";
        //System.Security.Cryptography.MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        //byte[] passwordBytes = System.Text.Encoding.Unicode.GetBytes(password);
        //byte[] hashBytes = md5.ComputeHash(passwordBytes);
        //hashPassword = System.Text.Encoding.Unicode.GetString(hashBytes);
        //return hashPassword;

        return FormsAuthentication.HashPasswordForStoringInConfigFile(password, "sha1");
    }

    public string Encrypt(string input)
    {
        string output = "";
        
        DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        MemoryStream memoryStream = new MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(cryptBytes, cryptBytes), CryptoStreamMode.Write);
        StreamWriter writer = new StreamWriter(cryptoStream);

        writer.Write(input);
        writer.Flush();
        cryptoStream.FlushFinalBlock();
        writer.Flush();
        
        output = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        return output;
    }


    public string Decrypt(string input)
    {
        string output = "";

        DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
        MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(input));
        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateDecryptor(cryptBytes, cryptBytes), CryptoStreamMode.Read);
        StreamReader reader = new StreamReader(cryptoStream);
        
        output = reader.ReadToEnd();
        return output;
    }

    public int ExecuteNonQuery(string queryString)
    {
        Int32 returnValue;        

        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand command = new MySqlCommand(queryString, conn);

        try
        {           
            conn.Open();
            returnValue = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            returnValue = -1;

            if (log.isAppLoggingOn && log.isDBLoggingOn)
            {
                log.Log(ex);
            }
        }
        finally
        {
            conn.Close();
            conn = null;
        }
        return returnValue;
    }

    public int ExecuteScalar(string queryString)
    {
        int returnValue;
        
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand command = new MySqlCommand(queryString, conn);
        
        try
        {
            conn.Open();
            returnValue = Convert.ToInt32(command.ExecuteScalar());
        }
        catch (Exception ex)
        {
            returnValue = -1;
            if (log.isAppLoggingOn && log.isDBLoggingOn)
            {
                log.Log(ex);
            }
        }
        finally
        {
            conn.Close();
            conn = null;
        }
        return returnValue;
    }
        
    public MySqlDataReader ExecuteReader(string queryString)
    {
        MySqlConnection conn = new MySqlConnection(connStr);
        MySqlCommand command = new MySqlCommand(queryString, conn);

        MySqlDataReader dataReader = null;

        try
        {
            conn.Open();           
            dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            dataReader = null;
            if (log.isAppLoggingOn && log.isDBLoggingOn)
            {
                log.Log(ex);
            }
        }
        finally
        {
            //  conn.Close();
            //  conn = null;
        }
        return dataReader;
    }   
}
