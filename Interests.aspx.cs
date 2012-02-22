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
using System.Collections.Specialized;
using MySql.Data.MySqlClient;

public partial class Interests : System.Web.UI.Page
{
    DBOperations dbOps;
    GUIVariables gui;
    Links links;
    ProcessingEngine engine;

    string username;
        
    
    //  <!-- A portfolio can have a max. of maxStocksPerUser Stocks in the portfolio. -->
    //  <!-- Will help in Scaling. Can also do Paid Subscriptions later.-->
    int maxStocksPerUser = int.Parse(ConfigurationManager.AppSettings["maxStocksPerUser"]);
       

    protected void Page_Load(object sender, EventArgs e)
    {           
        dbOps = (DBOperations)Application["dbOps"];
        gui = (GUIVariables)Application["gui"];
        links = (Links)Application["links"];
        engine = (ProcessingEngine)Application["engine"];

        //  username = Convert.ToString(Session["username"]);

        if (Request.Cookies["stoockerCookie"] != null)
        {
            HttpCookie stoockerCookie = Request.Cookies["stoockerCookie"];
            username = dbOps.Decrypt(stoockerCookie["username"].ToString().Trim());
        }


        if (string.IsNullOrEmpty(username))
        {
            Response.Redirect(links.SessionExpiredPageLink);
        }
        
        FindStockTB.Attributes.Add("onkeypress", "if ((event.which ? event.which : event.keyCode) == 13){document.getElementById('" + FindStockButton.UniqueID + "').click(); }");

        //  InterestsLabel.Text = "Select the Stocks you would like to <font color=\"Red\">Stoock!!<\font>";
        InterestsLabel.Text = "Other User's have been making predictions for the stocks shown." 
            + "Select the Stocks you would like to " + gui.StoockFont
            + gui.LineBreak + "(Max. of " + gui.RedFontStart + maxStocksPerUser.ToString() + gui.RedFontEnd + " stocks allowed)";

        string findStockMessage = "Could not find the Symbol you were looking for?"
                + gui.LineBreak + gui.StoocksFont + " only shows the Stocks which other Stoockers have predicted."
                + gui.LineBreak  + "Type the Symbol of the Stock you want to analyze and " + gui.StoocksFont + " will add the Symbol for You.";
        FindStockLabel.Text = findStockMessage;
        
        if (!Page.IsPostBack)
        {
            FillStocksCBL();
            ShowAlreadySelectedStocks(username);
            
            //  AlphabeticButton.Attributes.Add("onmouseover", "this.style.backgroundColor='red'");            
            //  AlphabeticButton.Attributes.Add("onmouseout", "this.style.backgroundColor='white'");
        }

        for (int i = 0; i < StocksCBL.Items.Count; i++)
        {
            StocksCBL.Items[i].Attributes.Add("onmouseover", "this.style.backgroundColor='red'");
            StocksCBL.Items[i].Attributes.Add("onmouseout", "this.style.backgroundColor='white'");
        }

        FindStockTB.Attributes.Add("onkeypress", "if ((event.which ? event.which : event.keyCode) == 13){var sendElem = document.getElementById(\"FindStockButton\"); if(!sendElem.disabled) FindStockButton_Click(); }");
        
    }

    private void FillStocksCBL()
    {           
        //  string queryString = "SELECT CompanyName,StockSymbol FROM Stoocks.Company";
        string queryString = "SELECT * FROM stoocks.company order by StockSymbol asc;";

        //  ArrayList retList = dbOps.QueryMultipleRowSelect(queryString);
        MySqlDataReader retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
        {
            //  for (int i = 0; i < retList.Count; i++)
            while(retList.Read())
            {
                //  StringCollection sc = (StringCollection)retList[i];
                string sc = retList.GetString(1);
                
                //  Show both Company Name and Symbol
                //  StocksCBL.Items.Add(new ListItem(sc[0] + " (" + sc[1] + ")", sc[1]));

                //  Show only Symbol
                //  if (!StocksCBL.Items.Contains(new ListItem(sc[1].ToUpper(),sc[1].ToUpper())))
                //  {
                    //  StocksCBL.Items.Add(new ListItem(sc[1].ToUpper(), sc[1].ToUpper()));                    
                //  }

                if (!StocksCBL.Items.Contains(new ListItem(sc.ToUpper(),sc.ToUpper())))
                {
                    StocksCBL.Items.Add(new ListItem(sc.ToUpper(), sc.ToUpper()));
                }
            }
            retList.Close();
        }

    }
       

    private void ShowAlreadySelectedStocks(string username)
    {
        string queryString = @"SELECT InterestedStocks FROM stoocks.interests WHERE Username='" + username + "' ;";

        string alreadySelectedStocks = "";
        //  alreadySelectedStocks = dbOps.QueryInformationSelect(queryString);
        
        MySqlDataReader retList = dbOps.ExecuteReader(queryString);

        if (retList != null && retList.HasRows)
        while (retList.Read())
        {
            alreadySelectedStocks = retList.GetString(0);
        } 
        retList.Close();

        //  Error Code for dbOps.QueryInformationSelect() is -1
        if (!string.IsNullOrEmpty(alreadySelectedStocks) && alreadySelectedStocks != "-1")
        {
            string[] splitter = { "," };
            string[] interestedStocks = alreadySelectedStocks.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < interestedStocks.Length; i++)
            {
                //    foreach (ListItem share in StocksCBL.Items)
                //    {
                //        if (share.Value.Equals(interestedStocks[i]))
                //        {
                //            share.Selected = true;
                //        }
                //        else
                //        {
                //            share.Selected = false;
                //        }
                //    }

                for (int j = 0; j < StocksCBL.Items.Count; j++)
                {
                    ListItem stocksCB = StocksCBL.Items.FindByValue(StocksCBL.Items[j].Value);
                    if (interestedStocks[i].Equals(StocksCBL.Items[j].Text))
                    {
                        stocksCB.Selected = true;                        
                    }                    
                }
            }

        }
    }


    protected void AddButton_Click(object sender, EventArgs e)
    {     
        string interestedShares = "";

        //  foreach (ListItem share in StocksLB.Items)
        //foreach (ListItem stock in StocksCBL.Items)
        //{
        //    if (stock.Selected == true)
        //    {
        //        if (interestedShares != "")
        //        {
        //            interestedShares = interestedShares + "," + stock.Value;
        //        }
        //        else
        //        {
        //            interestedShares = stock.Value;
        //        }
        //    }
        //}

        int selectedItemsCount = 0;
        for (int n = 0; n < StocksCBL.Items.Count; n++)
        {
            if (StocksCBL.Items[n].Selected)
            {
                selectedItemsCount++;
                if (selectedItemsCount > maxStocksPerUser)
                {
                    break;
                }
            }
        }

        if (selectedItemsCount > maxStocksPerUser)
        {
            SaveChangesLabel.Text = "Currently Stoocker allows a maximum of " + gui.RedFontStart + maxStocksPerUser.ToString() + gui.RedFontEnd + " stocks per portfolio."
                + gui.LineBreak + "Please choose accordingly.";                
        }
        else
        {

            string queryString = "INSERT IGNORE INTO stoocks.perstockexpertise VALUES ";
            StringCollection insertionValuesSC = new StringCollection();
            
            //  int defaultExpertise = 2;
            //  double defaultCurrentWeight = 100.0;

            int expertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;
            double userWeight = engine.InitialUserWeight;

            for (int i = 0; i < StocksCBL.Items.Count; i++)
            {
                if (StocksCBL.Items[i].Selected)
                {
                    if (interestedShares != "")
                    {
                        interestedShares = interestedShares + "," + StocksCBL.Items[i].Value;
                        insertionValuesSC.Add("('" + username + "', '" + StocksCBL.Items[i] + "', " + expertise + "," + userWeight + ") ");
                    }
                    else
                    {
                        interestedShares = StocksCBL.Items[i].Value;
                    }
                }
            }

            string insertionValuesString = "";
            for (int i = 0; i < insertionValuesSC.Count; i++)
            {
                if (i == (insertionValuesSC.Count - 1))
                {
                    insertionValuesString += insertionValuesSC[i] + ";";
                }
                else
                {
                    insertionValuesString += insertionValuesSC[i] + ",";
                }
            }

            //  Update perstockexpertise Table.
            queryString = queryString + insertionValuesString;
            int retVal = dbOps.ExecuteNonQuery(queryString);


            //  Response.Write("You Selected: " + interestedShares);
            //  System.Threading.Thread.Sleep(2000);

            interestedShares = SortAlphabetically(interestedShares);

            bool isUpdated = UpdateInterestsDB(username, interestedShares);
            if (isUpdated)
            {
                Response.Redirect(links.HomePageLink);
            }
        }
    }

    private string SortAlphabetically(string interestedShares)
    {
        string sortedInterestedShares = "";

        string[] splitter = { "," };
        string[] interestedStocks = interestedShares.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

        ArrayList interestedStocksList = new ArrayList();
        for (int i = 0; i < interestedStocks.Length; i++)
        {
            interestedStocksList.Add(interestedStocks[i]);
        }
        interestedStocksList.Sort();

        for (int i = 0; i < interestedStocksList.Count; i++)
        {
            sortedInterestedShares = sortedInterestedShares + "," + interestedStocksList[i].ToString();
        }

        //  The first character in sortedInterestedShares would be a ','. Remove it.
        return sortedInterestedShares.Substring(1);
    }


    private bool UpdateInterestsDB(string username, string interestedShares)
    {
        string queryString = @"SELECT Count(*) FROM stoocks.interests WHERE Username='" + username + "' ;";

        int retVal = dbOps.ExecuteScalar(queryString);
        if (retVal == 1)    //  Update
        {
            queryString = @"UPDATE stoocks.interests SET InterestedStocks = '" + interestedShares + "' WHERE Username='" + username + "';";
            retVal = dbOps.ExecuteNonQuery(queryString);                        
        }
        else    //  Insert
        {            
            queryString = @"INSERT INTO stoocks.interests VALUE ('" + username + "', '" + interestedShares + "');";
            retVal = dbOps.ExecuteNonQuery(queryString);    
        }
        
        if (retVal == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
                
    }

    protected void FindStockButton_Click(object sender, EventArgs e)
    {
        FindStock();
    }

    protected void FindStock()
    {
        FindStockMessageLabel.Text = "";
        string symbol = FindStockTB.Text.Trim();
        string errorMessage = "";

        if (!string.IsNullOrEmpty(symbol))
        {
            string companyName = "";
            StockService stockService = new StockService();

            //  If the User entered the Stock Symbol, then extract the Company Name.
            //  if (stockService.XIgniteIsQuoteExists(symbol, out companyName))
            if (stockService.YahooCSVIsQuoteExists(symbol, out companyName))
            {
                symbol = symbol.ToUpper();
                if (companyName == "")
                {
                    companyName = symbol;
                }

                string queryString = "INSERT IGNORE INTO stoocks.company VALUES ('" + companyName + "','" + symbol + "','');";
                int retVal = dbOps.ExecuteNonQuery(queryString);

                if (retVal != -1)
                {
                    if (!StocksCBL.Items.Contains(new ListItem(symbol, symbol)))
                    {
                        StocksCBL.Items.Add(new ListItem(symbol, symbol));
                        string message = gui.GreenFontStart + " Symbol found. Added to the list above." + gui.GreenFontEnd;
                        FindStockMessageLabel.Text = message;
                    }
                }
            }
            else
            {
                errorMessage = gui.RedFontStart + " * Symbol not found." + gui.RedFontEnd;
                FindStockMessageLabel.Text = errorMessage;
            }
        }
        else
        {
            errorMessage = gui.RedFontStart + " Please Enter a Valid Stock Symbol." + gui.RedFontEnd;
            FindStockMessageLabel.Text = errorMessage;
        }
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
    #endregion Footer Links
  
}
