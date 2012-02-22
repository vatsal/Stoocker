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

public partial class About : System.Web.UI.Page
{
    GUIVariables gui;

    protected void Page_Load(object sender, EventArgs e)
    {
        gui = (GUIVariables)Application["gui"];

        AboutLabel.Text = gui.GrayFontStart
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + gui.StoocksFont + " is a Stock Recommendation Engine." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "Helps you take informed decisions about stock investments." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "Maintain a " + gui.GreenFontStart + "Portfolio" + gui.GreenFontEnd + " of your stocks." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "Get updated " + gui.GreenFontStart + "News" + gui.GreenFontEnd + " about the latest happenings in the Stock Market." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + gui.GreenFontStart + "Predict" + gui.GreenFontEnd + " tomorrows movement of your favorite stocks." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "See what the other users " + gui.GreenFontStart + "Recommend" + gui.GreenFontEnd + " about the stock movement for tomorrow."
            + gui.GrayFontEnd;

        ExplanationLabel.Text = gui.LineBreak + gui.BoldFontStart + gui.RedFontStart + "How does the engine work?" + gui.RedFontEnd + gui.BoldFontEnd + gui.LineBreak
            + gui.GrayFontStart
            + gui.SmallFontStart + "Note: User A represents all users who use stoocker.com, and Stock X represents all the stocks that are currently being tracked by the users." + gui.SmallFontEnd + gui.LineBreak            
            + "(1) User A " + gui.BlueFontStart + "Registers" + gui.BlueFontEnd + " at stoocker.com." + gui.LineBreak
            + "(2) User A " + gui.BlueFontStart + "Creates a Portfolio" + gui.BlueFontEnd + " of the stocks he/she is interested in." + gui.LineBreak
            + "(3) User A can now Login and get" + gui.BlueFontStart + " Updated Stock Feeds" + gui.BlueFontEnd + " and " + gui.BlueFontStart + "Latest News" + gui.BlueFontEnd + " tailored according to User A's portfolio." + gui.LineBreak
            + "(4) User A can make an " + gui.BlueFontStart + "Informed Prediction" + gui.BlueFontEnd + " about whether the price of a particular stock (say stock X) will go up or will go down tomorrow." + gui.LineBreak
            + "(5) The stoocker Recommendation Engine takes this recommendation from User A and blends it into the combined predictions of all other Users who have also made a recommendation on Stock X. " + gui.LineBreak
            + "(6) The stoocker Engine uses its own mix of " + gui.BlueFontStart + "Machine Algorithms" + gui.BlueFontEnd + " which then make an inference on the movement of Stock X for tomorrow." + gui.LineBreak
            + "(7) This Prediction helps all the other Users who have stock X added to their portfolio, in making a decision on whether to buy or sell Stock X in the very-near future." + gui.LineBreak
            + "(8) The influence of User A (for Stock X) on the stoocker Recommendation Engine fluctuates according to the number of accurate predictions User A makes for Stock X. " + gui.LineBreak
            //+ "(8) If User A predicted the movement correctly, User A will carry a much higher weight the next time User A makes a prediction," + gui.LineBreak
            //+ "     but if User A makes an incorrect prediction, User A's influence on the stoocker Engine will be drastically reduced." + gui.LineBreak
            //+ "     Thus overtime, those who keep making correct recommendations keep on gaining higher respect from the algorithm and " + gui.LineBreak
            //+ "     Those who keep making false predictions get treated as spam and discarded." + gui.LineBreak
            + "(9) All in all, stoocker.com tries to " + gui.BlueFontStart + "Approximate who the best Investor's" + gui.BlueFontEnd + "  are and then makes a recommendation for short-term price movement changes based on the collaborative judgement and intuition of those experts." + gui.LineBreak
            + gui.GrayFontEnd;

        AdvantageLabel.Text = gui.LineBreak + gui.BoldFontStart + gui.RedFontStart + "Advantages" + gui.RedFontEnd + gui.BoldFontEnd + gui.LineBreak
            + gui.GrayFontStart
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + gui.GreenFontStart + "Extremely Simple" + gui.GreenFontEnd + " and " + "Easy to Use." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + gui.GreenFontStart + "Very Secure" + gui.GreenFontEnd + " (User's can only view their own portfolio and not the portfolio of others.)" + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + gui.GreenFontStart + "Scalable" + gui.GreenFontEnd + " (across 1000's of users with Accuracy of the Algorithm increasing in proportion to the number of users who use it everyday.)" + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "The system might just make you a lot of money (or save you a lot of money) if you keep tracking what the crowd thinks about the stocks you follow." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "Helps you " + gui.GreenFontStart + " Take the Right Investment Decision At The Right Time." + gui.GreenFontEnd + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "Unearths and Taps the intuition of the best investor's from a pool of large number of users. (Who knows, afterall your Mother might have a better judgement and intuition than you have." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + "If nothing else, it does improve your judgement and intuition." + gui.LineBreak
            + gui.RedFontStart + ">>" + gui.RedFontEnd + " " + gui.GreenFontStart + "Absolutely Free" + gui.GreenFontEnd + " (Unlike those sites which make you pay just for getting expert advice.)" + gui.LineBreak
            + gui.GrayFontEnd;


    }
}
