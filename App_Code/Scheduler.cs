using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Collections;
using System.Threading;
using System.Collections.Specialized;
using System.Diagnostics;
using MySql.Data.MySqlClient;
    
/// <summary>
/// Summary description for Scheduler
/// </summary>
public class Scheduler
{
    private SchedulerConfiguration configuration = null;
    Logger log = new Logger();
    ProcessingEngine engine = new ProcessingEngine();

    public Scheduler(SchedulerConfiguration config)
    {
        configuration = config;
    }

    public void Start()
    {
        while(true)
        {
            try
            {
                  //    For each job, call the execute method
                  foreach (ISchedulerJob job in configuration.Jobs)
                  {
                      job.Execute();                    
                  }
                  //  ((ISchedulerJob)configuration.Jobs[0]).Execute();
            }
            catch(Exception ex) 
            {
                if (log.isLoggingOn && log.isAppLoggingOn)
                {
                    log.Log(ex);
                }
            }
            finally
            {
                Thread.Sleep(configuration.SleepInterval);                
            }
        }
    }      
}

///<summary>
///Interface for Scheduler Jobs
///</summary>
public interface ISchedulerJob
{
    //  All the contract asks for is that a Job implements an Execute method that the scheduling engine can call. 
    void Execute();
}   

public class DoJob : ISchedulerJob
{
    DBOperations dbOps;
    StockService stockService;

    Logger log = new Logger();
    ProcessingEngine engine = new ProcessingEngine();

    private string dateFormatString = ConfigurationManager.AppSettings["dateFormatString"];
    private string smtpClient = ConfigurationManager.AppSettings["smtpClient"];
    private int schedulerTimeInHours = Convert.ToInt32(ConfigurationManager.AppSettings["schedulerTimeInHours"]);

    //  private double masterExpertisePercent = Convert.ToDouble(ConfigurationManager.AppSettings["masterExpertisePercent"]);
    //  private double expertExpertisePercent = Convert.ToDouble(ConfigurationManager.AppSettings["expertExpertisePercent"]);
    //  private double intermediateExpertisePercent = Convert.ToDouble(ConfigurationManager.AppSettings["intermediateExpertisePercent"]);

    string yesterday;
    string today;
    string tomorrow;
        
    ///<summary>
    ///A simple example of a job that sends out an email message.
    ///</summary>
    public void Execute()
    {
        //  IF Day == Saturday | Sunday Then Scheduler should run exactly once.
        //  Currently Not running the scheduler on Friday.               
        
        if ( (DateTime.Now.DayOfWeek != DayOfWeek.Saturday && DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
            && DateTime.Now.Hour == schedulerTimeInHours) //  This task has to run at 9:00 A.M. (EST)
        //  if ((DateTime.Now.Hour % 2) != 0) //  This task has to run at 12:00 A.M. (Midnight EST)
        {
            dbOps = new DBOperations();
            stockService = new StockService();

            yesterday = DateTime.Now.AddDays(-1).ToString(dateFormatString);
            today = DateTime.Now.ToString(dateFormatString);
            tomorrow = DateTime.Now.AddDays(+1).ToString(dateFormatString);

            //  Update the DB. This task has to run between 9:00 A.M. and 10:00 A.M. (Morning EST)
            UpdateDB();

            //  Log the time at which the Scheduler runs the Execute() Method 
            //  And the DB was Updated using the UpdateDB() Method.            
            log.Log("Run Execute(). DB Updated. Yay!");
        }        
    }

    /// <summary>
    /// Update the UserPrediction and StockPrediction and Expertise DB.
    /// </summary>
    private void UpdateDB()
    {        
        // UserPrediction(Username,PredictedMovement,Symbol,Time,Date)
        // StockPrediction(StockSymbol,Date,HighVotes,LowVotes,SumHighVotesMulWeight,SumLowVotesMulWeight,PredictedMovement,ActualMovement)
        // Expertise(Username,Expertise,CurrentWeight)

        /*
            Assuming that Yesterday, some user predicted Today's Price of a stock.
            StockSymbol, PredictedMovement = SELECT StockSymbol, PredictedMovement FROM StockPrediction WHERE Date = Today
            FOR EACH StockSymbol IN StockPrediction DB
                FETCH ActualMovement from Provider Services (Yahoo, XIgnite) for Today.
                IF PredictedMovement == ActualMovement
                        UPDATE UserPrediction SET CurrentWeight = CurrentWeight * 2 WHERE Symbol = Stock;
                ELSE
                        UPDATE UserPrediction SET CurrentWeight = CurrentWeight * 2 WHERE Symbol = Stock;
                END IF
            END LOOP               
        */

        //  DB Variables.
        string queryString = "";
        int retVal = 0;
        //  ArrayList retList;
        MySqlDataReader retList;

        //  Math Variables.
        string symbol = "";
        string predictedMovement = "";
        string actualMovement = "";
        string change = ""; 
        string currentPrice = "";
        string previousClose = "";

        queryString = @"SELECT StockSymbol,PredictedMovement FROM stoocks.stockprediction WHERE Date='" + today + "' ;";
        //  queryString = @"SELECT StockSymbol,PredictedMovement FROM Stoocks.StockPrediction WHERE Date='" + tomorrow + "' ;";
        
        //  retList = dbOps.QueryMultipleRowSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);
        
        //  for (int i = 0; (retList != null && retList.Count > 0) && i < retList.Count; i++)
        if(retList != null && retList.HasRows && !retList.IsClosed)
        while(retList.Read())
        {
            //  StringCollection sc = (StringCollection)retList[i];
            //  symbol = sc[0].ToString();
            //  predictedMovement = sc[1].ToString();

            symbol = retList.GetString(0);
            predictedMovement = retList.GetString(1);


            //  stockService.XIgniteGetQuoteData(symbol, out currentPrice, out change, out previousClose);
            stockService.YahooCSVGetQuoteData(symbol, out currentPrice, out change, out previousClose);

            if (change.StartsWith("-"))
            {
                //  actualMovement = "-1";
                actualMovement = ((int)ProcessingEngine.Movement.Down).ToString();
            }
            else
            {
                //  actualMovement = "1";
                actualMovement = ((int)ProcessingEngine.Movement.Up).ToString();
            }

            int pmResult;
            int amResult;            
            bool isAMInt = int.TryParse(actualMovement, out amResult);
            bool isPMInt = int.TryParse(predictedMovement, out pmResult);

            if (isAMInt && isPMInt)
            {
                //  Update the Actual Movement in the following 2 tables.
                //  UserPrediction()
                //  StockPrediction()

                queryString = "UPDATE stoocks.userprediction SET ActualMovement = " + amResult + " WHERE Symbol='" + symbol + "' AND Date='" + today + "';";
                retVal = dbOps.ExecuteNonQuery(queryString);

                queryString = "UPDATE stoocks.stockprediction SET ActualMovement = " + amResult + " WHERE StockSymbol='" + symbol + "' AND Date='" + today + "';";
                retVal = dbOps.ExecuteNonQuery(queryString);
                
                #region Older Code Updated 2007-09-25
                ////  if (actualMovement == predictedMovement)
                //if(amResult == pmResult)
                //{
                    
                //    //  queryString = "UPDATE Stoocks.Expertise SET CurrentWeight = CurrentWeight * 2 WHERE Username = (SELECT Username FROM Stoocks.Userprediction Where Symbol = (SELECT StockSymbol FROM Stoocks.Stockprediction WHERE PredictedMovement = ActualMovement AND Date = '" + today + "') AND Date = '" + today + "');";
                //    //  retVal = dbOps.QueryInsert(queryString);                    

                //    queryString = "SELECT StockSymbol FROM Stoocks.Stockprediction WHERE PredictedMovement = ActualMovement AND Date = '" + today + "';";
                //    ArrayList symbolList = dbOps.QueryMultipleRowSelect(queryString);

                //    for (int n = 0; (symbolList != null && symbolList.Count > 0) && n < symbolList.Count; n++)
                //    {
                //        StringCollection symbolSC = (StringCollection)symbolList[n];
                //        string currentSymbol = symbolSC[0];
                        
                //        queryString = "UPDATE Stoocks.Expertise SET CurrentWeight = CurrentWeight * 2 WHERE Username = (SELECT Username FROM Stoocks.Userprediction Where Symbol = '" + currentSymbol + "' AND PredictedMovement = '" + pmResult + "'AND Date = '" + today + "');";
                //        retVal = dbOps.QueryInsert(queryString);
                //    }
                    
                //}
                //else
                //{
                //    //  queryString = "UPDATE Stoocks.Expertise SET CurrentWeight = CurrentWeight / 2 WHERE Username = (SELECT Username FROM Stoocks.Userprediction Where Symbol = (SELECT StockSymbol FROM Stoocks.Stockprediction WHERE PredictedMovement = ActualMovement AND Date = '" + today + "') AND Date = '" + today + "');";
                //    //  retVal = dbOps.QueryInsert(queryString);
                    
                //    queryString = "SELECT StockSymbol FROM Stoocks.Stockprediction WHERE PredictedMovement = ActualMovement AND Date = '" + today + "';";
                //    ArrayList symbolList = dbOps.QueryMultipleRowSelect(queryString);

                //    for (int n = 0; (symbolList != null && symbolList.Count > 0) && n < symbolList.Count; n++)
                //    {
                //        StringCollection symbolSC = (StringCollection)symbolList[n];
                //        string currentSymbol = symbolSC[0];

                //        queryString = "UPDATE Stoocks.Expertise SET CurrentWeight = CurrentWeight / 2 WHERE Username = (SELECT Username FROM Stoocks.Userprediction Where Symbol = '" + currentSymbol + "' AND PredictedMovement = '" + pmResult + "' AND Date = '" + today + "');";
                //        retVal = dbOps.QueryInsert(queryString);
                //    }
                //}
                #endregion Older Code Updated 2007-09-25

                #region Update CurrentWeights

                //  Find all the users who predicted the movement correctly.
                //  Update the expertise and perstockexpertise tables by multiplying the currentweights by 2.
                queryString = "SELECT Username FROM stoocks.userprediction Where Symbol = '" + symbol + "' AND PredictedMovement = ActualMovement AND Date = '" + today + "';";
                
                //  ArrayList usernameList = dbOps.QueryMultipleRowSelect(queryString);
                MySqlDataReader usernameList = dbOps.ExecuteReader(queryString);

                string usersWhoPredictedCorrectly = "";

                //  for (int j = 0; (usernameList != null && usernameList.Count > 0) && j < usernameList.Count; j++)
                if (usernameList != null && usernameList.HasRows && !usernameList.IsClosed)
                while (usernameList.Read())
                {
                    //  StringCollection usernameSC = (StringCollection)usernameList[j];
                    //  usersWhoPredictedCorrectly = usernameSC[0];

                    usersWhoPredictedCorrectly = usernameList.GetString(0);

                    queryString = "UPDATE stoocks.expertise SET CurrentWeight = CurrentWeight * 2 WHERE Username = '" + usersWhoPredictedCorrectly + "';";
                    retVal = dbOps.ExecuteNonQuery(queryString);

                    queryString = "UPDATE stoocks.perstockexpertise SET CurrentWeight = CurrentWeight * 2 WHERE Username = '" + usersWhoPredictedCorrectly + "' AND Symbol='" + symbol + "';";
                    retVal = dbOps.ExecuteNonQuery(queryString);                    
                }
                usernameList.Close();
                //  retList.Close();    This is shifted to outside the while loop.

                //  Find all the users who predicted the movement incorrectly.
                //  Update the expertise and perstockexpertise tables by dividing the currentweights by 2.
                
                queryString = "SELECT Username FROM stoocks.userprediction Where Symbol = '" + symbol + "' AND PredictedMovement <> ActualMovement AND Date = '" + today + "';";
                
                //  usernameList = dbOps.QueryMultipleRowSelect(queryString);
                usernameList = dbOps.ExecuteReader(queryString);
                
                string usersWhoPredictedIncorrectly = "";

                //  for (int j = 0; (usernameList != null && usernameList.Count > 0) && j < usernameList.Count; j++)
                if (usernameList != null && usernameList.HasRows)
                while (usernameList.Read())
                {
                    //  StringCollection usernameSC = (StringCollection)usernameList[j];
                    //  usersWhoPredictedIncorrectly = usernameSC[0];

                    usersWhoPredictedCorrectly = usernameList.GetString(0);

                    queryString = "UPDATE stoocks.expertise SET CurrentWeight = CurrentWeight / 2 WHERE Username = '" + usersWhoPredictedIncorrectly + "';";
                    retVal = dbOps.ExecuteNonQuery(queryString);

                    queryString = "UPDATE stoocks.perstockexpertise SET CurrentWeight = CurrentWeight / 2 WHERE Username = '" + usersWhoPredictedIncorrectly + "' AND Symbol='" + symbol + "';";
                    retVal = dbOps.ExecuteNonQuery(queryString);
                }
                usernameList.Close();
                
                #endregion Update CurrentWeights

            }                
        }
        retList.Close();

        #region Update Expertise Level
                
        string username = "";
        int oldExpertise;
        int newExpertise;

        int correctPredictions;
        int totalPredictions;
        double percentPrediction;

        queryString = "SELECT distinct Username FROM stoocks.userprediction;";
        //  retList = dbOps.QueryMultipleRowSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);
        
        //  for (int i = 0; (retList != null && retList.Count > 0) && i < retList.Count; i++)
        if(retList != null && retList.HasRows)
        while(retList.Read())
        {
            //  StringCollection sc = (StringCollection)retList[i];
            //  username = sc[0].ToString().Trim();

            username = retList.GetString(0).Trim();

            queryString = "SELECT Expertise FROM stoocks.expertise WHERE Username='" + username + "';";
            oldExpertise = dbOps.ExecuteScalar(queryString);

            queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND ActualMovement=PredictedMovement;";
            correctPredictions = dbOps.ExecuteScalar(queryString);

            queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "';";
            totalPredictions = dbOps.ExecuteScalar(queryString);

            if (correctPredictions > 0 && totalPredictions > 0 && oldExpertise > 0)
            {                
                //  newExpertise = 2;  //  Default Expertise = Intermediate;
                newExpertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise = Intermediate;

                percentPrediction = (double)((double)correctPredictions / (double)totalPredictions);
                if (percentPrediction >= engine.ExpertisePercentMaster) //  Master
                {
                    //  newExpertise = 4;
                    newExpertise = (int)ProcessingEngine.ExpertiseLevel.Master;
                }
                else if (percentPrediction >= engine.ExpertisePercentExpert) //  Expert
                {
                    //  newExpertise = 3;
                    newExpertise = (int)ProcessingEngine.ExpertiseLevel.Expert;
                }
                else if (percentPrediction < engine.ExpertisePercentIntermediate)    //  Beginner
                {
                    //  newExpertise = 1;
                    newExpertise = (int)ProcessingEngine.ExpertiseLevel.Beginner;
                }

                if (oldExpertise != newExpertise)
                {
                    queryString = "UPDATE stoocks.expertise SET Expertise = " + newExpertise + " WHERE Username = '" + username + "';";
                    retVal = dbOps.ExecuteNonQuery(queryString);
                }
            }
        }
        retList.Close();
        #endregion Update Expertise Level

        #region Update stoocks.peruserexpertise Expertise

        queryString = "SELECT Username, Symbol, Expertise FROM stoocks.perstockexpertise;";
        //  retList = dbOps.QueryMultipleRowSelect(queryString);
        retList = dbOps.ExecuteReader(queryString);

        //  for (int i = 0; (retList != null && retList.Count > 0) && i < retList.Count; i++)
        if(retList != null && retList.HasRows)
        while(retList.Read())
        {
            //StringCollection sc = (StringCollection)retList[i];
            //username = sc[0].ToString();
            //symbol = sc[1].ToString();
            //oldExpertise = Convert.ToInt32(sc[2]);

            username = retList.GetString(0);
            symbol = retList.GetString(1);
            oldExpertise = retList.GetInt32(2);
            
            queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND Symbol='" + symbol + "' AND ActualMovement=PredictedMovement;";
            correctPredictions = dbOps.ExecuteScalar(queryString);

            queryString = "SELECT Count(*) FROM stoocks.userprediction WHERE Username='" + username + "' AND Symbol='" + symbol + "';";
            totalPredictions = dbOps.ExecuteScalar(queryString);

            if (correctPredictions > 0 && totalPredictions > 0 && oldExpertise > 0)
            {                
                //  newExpertise = 2;  //  Default Expertise = Intermediate;
                newExpertise = (int)ProcessingEngine.ExpertiseLevel.Intermediate;  //  Default Expertise = Intermediate;

                percentPrediction = (double)((double)correctPredictions / (double)totalPredictions);
                if (percentPrediction >= engine.ExpertisePercentMaster) //  Master
                {
                    //  newExpertise = 4;
                    newExpertise = (int)ProcessingEngine.ExpertiseLevel.Master;
                }
                else if (percentPrediction >= engine.ExpertisePercentExpert) //  Expert
                {
                    //  newExpertise = 3;
                    newExpertise = (int)ProcessingEngine.ExpertiseLevel.Expert;
                }
                else if (percentPrediction < engine.ExpertisePercentIntermediate)    //  Beginner
                {
                    //  newExpertise = 1;
                    newExpertise = (int)ProcessingEngine.ExpertiseLevel.Beginner;
                }

                if (oldExpertise != newExpertise)
                {
                    queryString = "UPDATE stoocks.perstockexpertise SET Expertise = " + newExpertise + " WHERE Username = '" + username + "' AND symbol='" + symbol + "';";
                    retVal = dbOps.ExecuteNonQuery(queryString);
                }
            }
        }
        retList.Close();
        #endregion Update stoocks.peruserexpertise Expertise    
    }   
    
}

///<summary>
///Scheduler Configuration. The scheduling engine needs to know what jobs it has to run and how often. 
///</summary>
public class SchedulerConfiguration
{
    private int sleepInterval;
    private ArrayList jobs = new ArrayList();

    public SchedulerConfiguration(int newSleepInterval)
    {
        sleepInterval = newSleepInterval;
    }

    public int SleepInterval
    {
        get
        {
            return sleepInterval;
        }
    }

    public ArrayList Jobs
    {
        get
        {
            return jobs;
        }
    }    
}