<%@ Application Language="C#" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.Security" %>

<script runat="server">
   
    public DBOperations dbOps;
    public Logger log;
    public GUIVariables gui;
    public StockService stockService;
    public RssReader rssReader;
    public Links links;
    public General general;
    public ProcessingEngine engine;
    
    private System.Threading.Thread schedulerThread = null;
    private int scheduleEveryNSeconds = int.Parse(ConfigurationManager.AppSettings["scheduleEveryNSeconds"]);
            
    void Application_Start(object sender, EventArgs e) 
    {
        LoadClasses();        
        // Code that runs on application startup        
        LaunchScheduler();        
    }

    void LoadClasses()
    {
        dbOps = new DBOperations();
        log = new Logger();
        gui = new GUIVariables();
        stockService = new StockService();
        rssReader = new RssReader();
        links = new Links();
        general = new General();
        engine = new ProcessingEngine();
        
        Application["dbOps"] = dbOps;
        Application["log"] = log;
        Application["gui"] = gui;
        Application["stockService"] = stockService;
        Application["rssReader"] = rssReader;
        Application["links"] = links;
        Application["general"] = general;
        Application["engine"] = engine;
    }    

    /// <summary>
    /// The Scheduling Engine that assigns daily tasks to the worker thread. 
    /// Includes Updation of the UserPrediction and StockPrediction DB.
    /// </summary>
    void LaunchScheduler()
    {        
        //  SchedulerConfiguration sConfig = new SchedulerConfiguration(24 * 60 * 60 * 1000);
        SchedulerConfiguration sConfig = new SchedulerConfiguration(scheduleEveryNSeconds);
        
        sConfig.Jobs.Add(new DoJob());
        Scheduler scheduler = new Scheduler(sConfig);
        
        System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(scheduler.Start);
        schedulerThread = new System.Threading.Thread(threadStart);
        schedulerThread.Start();
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

        //  Finally, in order to shut the scheduler down, we need to call Abort() on the scheduler thread.
        if (null != schedulerThread)
        {
            schedulerThread.Abort();
        }
        
    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }

    void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }
       
</script>
