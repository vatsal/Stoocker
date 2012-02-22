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
using System.Collections.Specialized;

using MySql.Data.MySqlClient;

/// <summary>
/// Summary description for ProcessingEngine
/// </summary>
public class ProcessingEngine
{
    DBOperations dbOps;    
    Logger log;

    private int _initialExpertise;
    private int _initialPerStockExpertise;

    private double _initialUserWeight = double.Parse(ConfigurationManager.AppSettings["initialUserWeight"]);
    private double _initialPerStockUserWeight = double.Parse(ConfigurationManager.AppSettings["initialPerStockUserWeight"]);

    private double _expertisePercentIntermediate = Convert.ToDouble(ConfigurationManager.AppSettings["expertisePercentIntermediate"]);
    private double _expertisePercentExpert = Convert.ToDouble(ConfigurationManager.AppSettings["expertisePercentExpert"]);
    private double _expertisePercentMaster = Convert.ToDouble(ConfigurationManager.AppSettings["expertisePercentMaster"]);



    public double InitialExpertise
    {
        get
        {
            return _initialExpertise;
        }
    }

    public double InitialPerStockExpertise
    {
        get
        {
            return _initialPerStockExpertise;
        }
    }

    public double InitialUserWeight
    {
        get
        {
            return _initialUserWeight;
        }
    }

    public double InitialPerStockUserWeight
    {
        get
        {
            return _initialPerStockUserWeight;
        }
    }


    public double ExpertisePercentIntermediate
    {
        get
        {
            return _expertisePercentIntermediate;
        }
    }

    public double ExpertisePercentExpert
    {
        get
        {
            return _expertisePercentExpert;
        }
    }

    public double ExpertisePercentMaster
    {
        get
        {
            return _expertisePercentMaster;
        }
    }


	public ProcessingEngine()
	{
        dbOps = new DBOperations();
        log = new Logger();

        _initialExpertise = (int)ExpertiseLevel.Intermediate;
        _initialPerStockExpertise = (int)ExpertiseLevel.Intermediate;
        
	}

    public enum ExpertiseLevel
    {
        Beginner = 1, 
        Intermediate = 2, 
        Expert = 3, 
        Master = 4
    };
    
    public enum Movement
    {
        Neutral = 0,
        Up = 1,
        Down = -1
    };

    public enum PriceChange
    {
        VeryLow = -2,
        Low = -1,
        Same = 0,
        High = 1,
        VeryHigh = 2
    };

    public enum TradeSignal
    {
        Buy = -1,
        Hold = 1,
        Sell = 2
    }
   
}
