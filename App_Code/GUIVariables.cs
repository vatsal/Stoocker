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


/// <summary>
/// Summary description for GUIVariables
/// </summary>
public class GUIVariables
{
    private string stoocksFont;
    private string stoockFont;

    private string lineBreak;
    private string htmlSpace;

    private string boldFontStart;
    private string boldFontEnd;

    private string redFontStart;
    private string redFontEnd;

    private string greenFontStart;
    private string greenFontEnd;

    private string blueFontStart;
    private string blueFontEnd;

    private string grayFontStart;
    private string grayFontEnd;

    private string smallFontStart;
    private string smallFontEnd;

    public string StoocksFont
    {
        get { return stoocksFont; }
        set { stoocksFont = value; }
    }

    public string StoockFont
    {
        get { return stoockFont; }
        set { stoockFont = value; }
    }

    public string LineBreak
    {
        get { return lineBreak; }
        set { lineBreak = value; }
    }

    public string HTMLSpace
    {
        get { return htmlSpace; }
        set { htmlSpace = value; }
    }

    public string BoldFontStart
    {
        get { return boldFontStart; }
        set { boldFontStart = value; }
    }

    public string BoldFontEnd
    {
        get { return boldFontEnd; }
        set { boldFontEnd = value; }
    }

    public string RedFontStart
    {
        get { return redFontStart; }
        set { redFontStart = value; }
    }

    public string RedFontEnd
    {
        get { return redFontEnd; }
        set { redFontEnd = value; }
    }

    public string GreenFontStart
    {
        get { return greenFontStart; }
        set { greenFontStart = value; }
    }

    public string GreenFontEnd
    {
        get { return greenFontEnd; }
        set { greenFontEnd = value; }
    }

    public string BlueFontStart
    {
        get { return blueFontStart; }
        set { blueFontStart = value; }
    }

    public string BlueFontEnd
    {
        get { return blueFontEnd; }
        set { blueFontEnd = value; }
    }


    public string GrayFontStart
    {
        get { return grayFontStart; }
        set { grayFontStart = value; }
    }

    public string GrayFontEnd
    {
        get { return grayFontEnd; }
        set { grayFontEnd = value; }
    }

    public string SmallFontStart
    {
        get { return smallFontStart; }
        set { smallFontStart = value; }
    }

    public string SmallFontEnd
    {
        get { return smallFontEnd; }
        set { smallFontEnd = value; }
    }


	public GUIVariables()
	{
        LoadGUIVariables();
	}

    /// <summary>
    /// Load the GUI Variables like RGB Colored Fonts, Bold Font and LineBreaks.
    /// </summary>
    private void LoadGUIVariables()
    {
         stoocksFont = "<b><font color=red> Stoocker </font>!</b>";
         stoockFont = "<b><font color=red> Stoock </font>!</b>";

         lineBreak = "<br />";
         htmlSpace = "&nbsp;";                        

         boldFontStart = "<b>";
         boldFontEnd = "</b>";

         redFontStart = "<font color=red>";
         redFontEnd = "</font>";

         greenFontStart = "<font color=green>";
         greenFontEnd = "</font>";

         blueFontStart = "<font color=blue>";
         blueFontEnd = "</font>";

         grayFontStart = "<font color=gray>";
         grayFontEnd = "</font>";

         smallFontStart = "<font size=1.8>";
         smallFontEnd = "</font>";
    }

    

    

}
