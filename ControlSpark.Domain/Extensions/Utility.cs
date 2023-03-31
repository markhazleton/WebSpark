using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Drawing;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ControlSpark.Domain.Extensions;
public static class Utility
{
    public static string wpm_ApplyHTMLFormatting(string strInput)
    {
        strInput = $"~{strInput}";
        strInput = strInput.Replace(",", "-");
        strInput = strInput.Replace("'", "&quot;");
        strInput = strInput.Replace("\"", "&quot;");
        strInput = strInput.Replace("~", string.Empty);
        // strInput = Replace(strInput, " " , "_")
        return strInput;
    }
    public static bool wpm_Build301Redirect(string sNewURL)
    {
        //HttpContext.Current.Response.Status = "301 Moved Permanently";
        //HttpContext.Current.Response.AddHeader("Location", sNewURL);
        return true;
    }
    public static bool wpm_CheckForMatch(string StringOne, string StringTwo)
    {
        bool bMatch = false;
        if (StringOne is null | StringTwo is null)
        {
            if (StringOne is null & StringTwo is null)
            {
                bMatch = true;
            }
        }
        else
        {
            // To Make this Easier, let's ignore case and spaces and ampersands and dashes
            StringOne = StringOne.ToLower();
            StringOne = StringOne.Replace("/", string.Empty);
            StringOne = StringOne.Replace(".html", string.Empty);
            StringOne = StringOne.Replace(".htm", string.Empty);
            StringOne = StringOne.Replace("&amp;", "&");
            StringOne = StringOne.Replace("%20", string.Empty);
            StringOne = StringOne.Replace("-", string.Empty);
            StringOne = StringOne.Replace(" ", string.Empty);
            StringOne = StringOne.Replace(".html", string.Empty);
            StringTwo = StringTwo.ToLower();
            StringTwo = StringTwo.Replace("/", string.Empty);
            StringTwo = StringTwo.Replace(".html", string.Empty);
            StringTwo = StringTwo.Replace(".htm", string.Empty);
            StringTwo = StringTwo.Replace("%20", string.Empty);
            StringTwo = StringTwo.Replace(" ", string.Empty);
            StringTwo = StringTwo.Replace("&amp;", "&");
            StringTwo = StringTwo.Replace("-", string.Empty);
            StringTwo = StringTwo.Replace(".html", string.Empty);
            if ((StringOne ?? string.Empty) == (StringTwo ?? string.Empty))
            {
                bMatch = true;
            }
            else
            {
                bMatch = false;
            }
        }
        return bMatch;
    }
    public static string wpm_ClearLineFeeds(string sTextToCovert)
    {
        sTextToCovert = sTextToCovert.Replace(Environment.NewLine, string.Empty);
        sTextToCovert = sTextToCovert.Replace(Environment.NewLine, string.Empty);
        return sTextToCovert;
    }
    public static string wpm_ConvertRichTextToHTML(string sTextToCovert)
    {
        var sbReturn = new StringBuilder(WebUtility.HtmlEncode(sTextToCovert));
        sbReturn.Replace("'", "&#39;");
        sbReturn.Replace("  ", " &nbsp;");
        sbReturn.Replace(" & ", " &amp; ");
        sbReturn.Replace(Environment.NewLine, "<br/>");
        sbReturn.Replace(Environment.NewLine, "<br/>");
        return sbReturn.ToString();
    }
    public static string wpm_DaysInMonth()
    {
        string wpm_DaysInMonthRet = default;
        int lLngMonth = Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(DateTime.Now);
        switch (lLngMonth)
        {
            case 9:
            case 4:
            case 6:
            case 11:
                {
                    return "30";
                }
            case 2:
                {
                    if (wpm_IsNowLeapYear())
                        wpm_DaysInMonthRet = "29";
                    else
                        wpm_DaysInMonthRet = "28";
                    break;
                }

            default:
                {
                    return "31";
                }
        }

        return wpm_DaysInMonthRet;
    } // DaysInMonth()

    public static string wpm_FixInvalidCharacters(string Value)
    {
        if (Value is null)
        {
            Value = string.Empty;
        }
        else
        {
            Value = Value.Trim().ToLower();
            Value = Value.Replace(" & ", "-and-");
            Value = Value.Replace("&", "-and-");
            Value = Value.Replace(" | ", "-and-");
            Value = Value.Replace("|", "-and-");
            Value = Value.Replace(",", "-");
            Value = Value.Replace("/", "-");
            Value = Value.Replace(@"\", "-");
            Value = Value.Replace("<", "-");
            Value = Value.Replace(">", "-");
            Value = Value.Replace("(", "-");
            Value = Value.Replace(")", "-");
            Value = Value.Replace("[", "-");
            Value = Value.Replace("]", "-");
            Value = Value.Replace("{", "-");
            Value = Value.Replace("}", "-");
            Value = Value.Replace(",", "-");
            Value = Value.Replace("'", "-");
            Value = Value.Replace(";", "-");
            Value = Value.Replace(":", "-");
            Value = Value.Replace(" ", "-");
            Value = Value.Replace(Environment.NewLine, " ");
            Value = Value.Replace(Environment.NewLine, " ");
        }
        return Value;
    }
    public static string wpm_FixSingleQuote(string FixString)
    {
        var strFix = new StringBuilder(FixString);
        strFix.Replace("\"", "&quot;");
        strFix.Replace("''", "&#39;");
        strFix.Replace("'", "&#39;");
        // strFix.Replace(vbCrLf, "<br/>")
        return strFix.ToString();
    }
    public static string wpm_FormatDate(DateTime lDtmNow, ref string pStrDate)
    {
        string wpm_FormatDateRet = default;
        // Define local variables
        var lObjRegExp = new Regex("([a-z][a-z]*[a-z])*[a-z]", RegexOptions.IgnoreCase);
        int lLngHour = Thread.CurrentThread.CurrentCulture.Calendar.GetHour(lDtmNow);
        int lLngWeekDay = (int)Thread.CurrentThread.CurrentCulture.Calendar.GetDayOfWeek(lDtmNow);
        int lLngSecond = Thread.CurrentThread.CurrentCulture.Calendar.GetSecond(lDtmNow);
        int lLngMinute = Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(lDtmNow);
        int lLngDay = Thread.CurrentThread.CurrentCulture.Calendar.GetDayOfMonth(lDtmNow);
        int lLngMonth = Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(lDtmNow);
        int lLngYear = Thread.CurrentThread.CurrentCulture.Calendar.GetYear(lDtmNow);

        // Prepare RegExp object and set parameters

        // List each individual match and compare to different date functions
        foreach (Match lObjMatch in lObjRegExp.Matches(pStrDate))
        {
            switch (lObjMatch.Value ?? string.Empty)
            {
                case "a":
                    {
                        pStrDate = pStrDate.Replace("a", Strings.Right(lDtmNow.ToString().ToLower(), 2));
                        break;
                    }
                case "A":
                    {
                        pStrDate = Strings.Replace(pStrDate, "A", Strings.Right(lDtmNow.ToString().ToUpper(), 2));
                        break;
                    }
                case "B":
                    {
                        pStrDate = Strings.Replace(pStrDate, "B", wpm_InternetTime());
                        break;
                    }
                case "d":
                    {
                        pStrDate = Strings.Replace(pStrDate, "d", wpm_LeadingZero(ref lLngDay));
                        break;
                    }
                case "D":
                    {
                        pStrDate = Strings.Replace(pStrDate, "D", Strings.Left(DateAndTime.WeekdayName(lLngWeekDay), 3));
                        break;
                    }
                case "M":
                    {
                        pStrDate = Strings.Replace(pStrDate, "M", DateAndTime.MonthName(lLngMonth));
                        break;
                    }
                case "g":
                    {
                        pStrDate = Strings.Replace(pStrDate, "g", wpm_FormatHour().ToString());
                        break;
                    }
                case "G":
                    {
                        pStrDate = Strings.Replace(pStrDate, "G", lLngHour.ToString());
                        break;
                    }
                case "h":
                    {
                        string localwpm_LeadingZero() { int argpStrValue = wpm_FormatHour(); var ret = wpm_LeadingZero(ref argpStrValue); return ret; }

                        pStrDate = Strings.Replace(pStrDate, "h", localwpm_LeadingZero());
                        break;
                    }
                case "H":
                    {
                        pStrDate = Strings.Replace(pStrDate, "H", wpm_LeadingZero(ref lLngHour));
                        break;
                    }
                case "i":
                    {
                        pStrDate = Strings.Replace(pStrDate, "i", wpm_LeadingZero(ref lLngMinute));
                        break;
                    }
                case "j":
                    {
                        pStrDate = Strings.Replace(pStrDate, "j", lLngDay.ToString());
                        break;
                    }
                case "l":
                    {
                        pStrDate = Strings.Replace(pStrDate, "l", DateAndTime.WeekdayName(lLngWeekDay));
                        break;
                    }
                case "L":
                    {
                        pStrDate = Strings.Replace(pStrDate, "L", wpm_IsNowLeapYear().ToString());
                        break;
                    }
                case "m":
                    {
                        pStrDate = Strings.Replace(pStrDate, "m", wpm_LeadingZero(ref lLngMonth));
                        break;
                    }
                case var @case when @case == "M":
                    {
                        pStrDate = Strings.Replace(pStrDate, "M", Strings.Left(DateAndTime.MonthName(lLngMonth), 3));
                        break;
                    }
                case "n":
                    {
                        pStrDate = Strings.Replace(pStrDate, "n", lLngMonth.ToString());
                        break;
                    }
                case "r":
                    {
                        pStrDate = Strings.Replace(pStrDate, "r", string.Format("{0}, {1} {2} {3} {4}:{5}", Strings.Left(DateAndTime.WeekdayName(lLngWeekDay), 3), lLngDay, Strings.Left(DateAndTime.MonthName(lLngMonth), 3), lLngYear, Strings.FormatDateTime(DateAndTime.TimeOfDay, DateFormat.LongTime), wpm_LeadingZero(ref lLngSecond)));
                        break;
                    }
                case "s":
                    {
                        pStrDate = Strings.Replace(pStrDate, "s", wpm_LeadingZero(ref lLngSecond));
                        break;
                    }
                case "S":
                    {
                        pStrDate = Strings.Replace(pStrDate, "S", wpm_OrdinalSuffix());
                        break;
                    }
                case "t":
                    {
                        pStrDate = Strings.Replace(pStrDate, "t", wpm_DaysInMonth());
                        break;
                    }
                case "U":
                    {
                        pStrDate = Strings.Replace(pStrDate, "U", DateAndTime.DateDiff(DateInterval.Second, DateAndTime.DateSerial(1970, 1, 1), lDtmNow).ToString());
                        break;
                    }
                case "w":
                    {
                        pStrDate = Strings.Replace(pStrDate, "w", (lLngWeekDay - 1).ToString());
                        break;
                    }
                case "W":
                    {
                        pStrDate = Strings.Replace(pStrDate, "W", "1");
                        break;
                    }
                case "Y":
                    {
                        pStrDate = Strings.Replace(pStrDate, "Y", lLngYear.ToString());
                        break;
                    }
                case "y":
                    {
                        pStrDate = Strings.Replace(pStrDate, "y", Strings.Right(lLngYear.ToString(), 2));
                        break;
                    }
                case "z":
                    {
                        pStrDate = Strings.Replace(pStrDate, "z", "1");
                        break;
                    }

                default:
                    {
                        pStrDate = $"{pStrDate}";
                        break;
                    }
            }
        }
        lObjRegExp = null;
        wpm_FormatDateRet = pStrDate;
        return wpm_FormatDateRet;
    }
    // Accepts strDate as a valid date/time,
    // strFormat as the output template.
    // The function finds each item in the
    // template and replaces it with the
    // relevant information extracted from strDate

    // SiteTemplate items (example)
    // %m Month as a decimal (02)
    // %B Full month name (February)
    // %b Abbreviated month name (Feb )
    // %d Day of the month (23)
    // %O Ordinal of day of month (eg st or rd or nd)
    // %j Day of the year (54)
    // %Y Year with century (1998)
    // %y Year without century (98)
    // %w Weekday as integer (0 is Sunday)
    // %a Abbreviated day name (Fri)
    // %A Weekday Name (Friday)
    // %H Hour in 24 hour format (24)
    // %h Hour in 12 hour format (12)
    // %N Minute as an integer (01)
    // %n Minute as optional if minute <> 0
    // %S Second as an integer (55)
    // %P AM/PM Indicator (PM)

    public static string wpm_FormatDateString(string strDate, string strFormat)
    {
        ;
        var mySB = new StringBuilder(strFormat);
        int int12HourPart = new int();
        string str24HourPart = string.Empty;
        string strMinutePart = string.Empty;
        string strSecondPart = string.Empty;
        string strAMPM = string.Empty;

        // Insert Month Numbers
        mySB.Replace("%m", DateAndTime.DatePart("m", strDate).ToString());

        // Insert non-Abbreviated Month Names
        mySB.Replace("%B", DateAndTime.MonthName(DateAndTime.DatePart("m", strDate), false));

        // Insert Abbreviated Month Names
        mySB.Replace("%b", DateAndTime.MonthName(DateAndTime.DatePart("m", strDate), true));

        // Insert Day Of Month
        mySB.Replace("%d", DateAndTime.DatePart("d", strDate).ToString());

        // Insert Day of Month Ordinal (eg st, th, or rd)
        mySB.Replace("%O", wpm_GetDayOrdinal(Thread.CurrentThread.CurrentCulture.Calendar.GetDayOfMonth(Conversions.ToDate(strDate))));

        // Insert Day of Year
        mySB.Replace("%j", DateAndTime.DatePart("y", Conversions.ToDate(strDate)).ToString());

        // Insert Long Year (4 digit)
        mySB.Replace("%Y", DateAndTime.DatePart("yyyy", Conversions.ToDate(strDate)).ToString());

        // Insert Short Year (2 digit)
        mySB.Replace("%y", Strings.Right(DateAndTime.DatePart("yyyy", Conversions.ToDate(strDate)).ToString(), 2));

        // Insert Weekday as Integer (eg 0 = Sunday)
        mySB.Replace("%w", DateAndTime.DatePart("w", Conversions.ToDate(strDate), FirstDayOfWeek.Monday, FirstWeekOfYear.Jan1).ToString());

        // Insert Abbreviated Weekday Name (eg Sun)
        mySB.Replace("%a", DateAndTime.WeekdayName(DateAndTime.DatePart("w", strDate, FirstDayOfWeek.Monday, FirstWeekOfYear.Jan1), true));

        // Insert non-Abbreviated Weekday Name
        mySB.Replace("%A", DateAndTime.WeekdayName(DateAndTime.DatePart("w", strDate, FirstDayOfWeek.Monday, FirstWeekOfYear.Jan1), false));

        // Insert Hour in 24hr format
        str24HourPart = DateAndTime.DatePart("h", Conversions.ToDate(strDate)).ToString();
        if (Strings.Len(str24HourPart) < 2)
        {
            str24HourPart = $"0{str24HourPart}";
        }
        mySB.Replace("%H", str24HourPart);

        // Insert Hour in 12hr format
        int12HourPart = DateAndTime.DatePart("h", strDate) % 12;
        if (int12HourPart == 0)
        {
            int12HourPart = 12;
        }
        mySB.Replace("%h", int12HourPart.ToString());

        // Insert Minutes
        strMinutePart = DateAndTime.DatePart("n", Conversions.ToDate(strDate)).ToString();
        if (Strings.Len(strMinutePart) < 2)
        {
            strMinutePart = $"0{strMinutePart}";
        }
        mySB.Replace("%N", strMinutePart);

        // Insert Optional Minutes
        if (Conversions.ToInteger(strMinutePart) == 0)
        {
            mySB.Replace("%n", string.Empty);
        }
        else
        {
            if (Conversions.ToInteger(strMinutePart) < 10)
            {
                strMinutePart = $"0{strMinutePart}";
            }
            strMinutePart = $":{strMinutePart}";
            mySB.Replace("%n", strMinutePart);
        }

        // Insert Seconds
        strSecondPart = DateAndTime.DatePart("s", Conversions.ToDate(strDate)).ToString();
        if (Strings.Len(strSecondPart) < 2)
            strSecondPart = $"0{strSecondPart}";
        mySB.Replace("%S", strSecondPart);

        // Insert AM/PM indicator
        if (DateAndTime.DatePart("h", strDate) >= 12)
        {
            strAMPM = "PM";
        }
        else
        {
            strAMPM = "AM";
        }

        mySB.Replace("%P", strAMPM);

        // If there is an error output its value
        if (Information.Err().Number != 0)
        {
            mySB.Append("ERROR in fncFmtDate: " + Information.Err().Description);
        }
        return mySB.ToString();
    } // fncFmtDate
    public static int wpm_FormatHour()
    {
        string lDtmNow = Strings.FormatDateTime(DateTime.Now, DateFormat.LongTime);
        return Conversions.ToInteger(Strings.Left(lDtmNow, Strings.InStr(lDtmNow, ":") - 1));
    } // FormatHour()
    public static string wpm_FormatLink(string LinkName, string LinkType, string LinkURL)
    {
        string sReturn;
        switch (LinkType ?? string.Empty)
        {
            case "Contact":
                {
                    sReturn = LinkName;
                    break;
                }

            default:
                {
                    sReturn = $"<a href=\"{LinkURL}\">{LinkName}</a>";
                    break;
                }
        }
        return sReturn;
    }
    public static string wpm_FormatLink(string LinkName, string LinkType, string LinkURL, string LinkDescription)
    {
        string sReturn;
        switch (LinkType ?? string.Empty)
        {
            case "Contact":
                {
                    sReturn = LinkName;
                    break;
                }

            default:
                {
                    sReturn = $"<a href=\"{LinkURL}\" title=\"{LinkDescription}\">{LinkName}</a>";
                    break;
                }
        }
        return sReturn;
    }
    public static string wpm_FormatNameForURL(string Name)
    {
        Name = Strings.Replace(Name.ToLower(), " ", "-");
        Name = Strings.Replace(Name, "(", "-");
        Name = Strings.Replace(Name, ")", "-");
        return Name;
        // Return FixInvalidCharacters(Name)
    }
    public static string wpm_FormatPageNameForURL(string PageName)
    {
        if (PageName is null)
        {
            return null;
        }
        else
        {
            return wpm_FixInvalidCharacters(PageName.ToLower());
        }
    }
    public static string wpm_FormatPageNameLink(string sPageName)
    {
        string sReturn = string.Empty;
        if (string.IsNullOrEmpty(Strings.Trim(sPageName)))
        {
            sReturn = "<a href=\"/\">Home Page</a>";
        }
        else
        {
            sReturn = $"<a href=\"{wpm_FormatPageNameURL(sPageName)}\">{sPageName}</a>";
        }
        return sReturn.ToLower();
    }
    public static string wpm_FormatPageNameURL(string sPageName)
    {
        if (string.IsNullOrEmpty(Strings.Trim(sPageName)))
        {
            return "/";
        }
        else
        {
            return string.Format("/{0}{1}", wpm_FixInvalidCharacters(sPageName), string.Empty);
        }
    }
    public static string wpm_FormatTextEntry(string strEntry)
    {
        if (string.IsNullOrEmpty(strEntry))
            strEntry = " ";
        strEntry = Strings.Replace(strEntry, "'", "&#39;");
        return strEntry;
    }
    public static string wpm_GetCurrentDate()
    {
        var dRightNow = DateTime.Now;
        return dRightNow.ToLongDateString();
    }
    public static string wpm_GetDayOrdinal(int intDay)
    {
        // Accepts a day of the month as an integer and returns the
        // appropriate suffix
        string strOrd = string.Empty;
        switch (intDay)
        {
            case 1:
            case 21:
            case 31:
                {
                    strOrd = "st";
                    break;
                }
            case 2:
            case 22:
                {
                    strOrd = "nd";
                    break;
                }
            case 3:
            case 23:
                {
                    strOrd = "rd";
                    break;
                }

            default:
                {
                    strOrd = "th";
                    break;
                }
        }
        return strOrd;
    } // fncGetDayOrdinal
    public static bool wpm_GetDBBoolean(object dbObject)
    {
        if (dbObject is DBNull)
        {
            return false;
        }
        else
        {
            return Conversions.ToBoolean(dbObject);
        }
    }
    // ********************************************************************************
    public static DateTime wpm_GetDBDate(object dbObject)
    {
        if (dbObject is DBNull)
        {
            return new DateTime();
        }
        else if (dbObject is null)
        {
            return new DateTime();
        }
        else if (string.IsNullOrWhiteSpace(dbObject.ToString()))
        {
            return new DateTime();
        }
        else if (wpm_IsDate(dbObject.ToString()))
        {
            return Conversions.ToDate(dbObject);
        }
        else
        {
            return new DateTime();
        }

    }
    public static double wpm_GetDBDouble(object dbObject)
    {
        if (dbObject is DBNull)
        {
            return default;
        }
        else if (dbObject is null)
        {
            return default;
        }
        else if (Information.IsNumeric(dbObject))
        {
            return Conversions.ToDouble(dbObject);
        }
        else
        {
            return default;
        }
    }
    public static int wpm_GetDBInteger(object dbObject)
    {
        if (dbObject is DBNull)
        {
            return default;
        }
        else if (dbObject is null)
        {
            return default;
        }
        else if (Information.IsNumeric(dbObject))
        {
            return Conversions.ToInteger(dbObject);
        }
        else
        {
            return default;
        }
    }
    public static int wpm_GetDBInteger(object dbObject, int DefaultValue)
    {
        if (dbObject is DBNull)
        {
            return DefaultValue;
        }
        else if (dbObject is null)
        {
            return DefaultValue;
        }
        else if (Information.IsNumeric(dbObject))
        {
            return Conversions.ToInteger(dbObject);
        }
        else
        {
            return DefaultValue;
        }
    }
    // ********************************************************************************
    public static string wpm_GetDBString(object dbObject)
    {
        string strEntry = string.Empty;
        if (!(dbObject is DBNull | dbObject is null))
        {
            strEntry = Conversions.ToString(dbObject);
            if (strEntry == " ")
                strEntry = string.Empty;
        }
        return strEntry;
    }
    public static string wpm_GetRFC822Date(object dbObject)
    {
        int offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
        string timeZone__1 = $"+{(offset.ToString().PadLeft(2, '0'))}";
        DateTime myDate;
        if (dbObject is DBNull)
        {
            myDate = DateTime.Now.AddDays(-100);
        }
        else
        {
            myDate = Conversions.ToDate(dbObject);
        }

        if (offset < 0)
        {
            int i = offset * -1;
            timeZone__1 = $"-{(i.ToString().PadLeft(2, '0'))}";
        }
        return myDate.ToString($"ddd, dd MMM yyyy HH:mm:ss {(timeZone__1.PadRight(5, '0'))}");
    }
    public static string wpm_GetStringValue(string myString)
    {
        if (myString is DBNull | myString is null)
        {
            myString = string.Empty;
        }
        return myString;
    }
    public static int wpm_HexStringToBase10Int(string hex)
    {
        int base10value = 0;
        try
        {
            base10value = Convert.ToInt32(hex, 16);
        }
        catch
        {
            base10value = 0;
        }
        return base10value;
    }
    public static Color wpm_HexStringToColor(string hex)
    {
        hex = hex.Replace("#", string.Empty);
        if (hex.Length != 6)
        {
            throw new Exception($"{hex} is not a valid 6-place hexadecimal color code.");
        }
        return Color.FromArgb(wpm_HexStringToBase10Int(hex.Substring(0, 2)), wpm_HexStringToBase10Int(hex.Substring(2, 2)), wpm_HexStringToBase10Int(hex.Substring(4, 2)));
    }
    public static string wpm_InternetTime()
    {
        float lLngTime = Thread.CurrentThread.CurrentCulture.Calendar.GetHour(DateAndTime.TimeOfDay) * 3600 * 1000 + Thread.CurrentThread.CurrentCulture.Calendar.GetMinute(DateAndTime.TimeOfDay) * 60 * 1000 + (Thread.CurrentThread.CurrentCulture.Calendar.GetSecond(DateAndTime.TimeOfDay) * 1000 + 3600000);
        float lLngBeats = lLngTime / 86400f;
        double lLngBeatsRound = Math.Round((double)lLngBeats);
        if (lLngBeatsRound > 1000d)
        {
            return $"@0{lLngBeatsRound}";
        }
        else if (lLngBeatsRound > 100d)
        {
            return $"@0{lLngBeatsRound}";
        }
        else
        {
            return $"@{lLngBeatsRound}";
        }
    } // InternetTime()

    public static bool wpm_IsDate(string strDate)
    {
        DateTime dtDate;
        bool bValid = true;
        try
        {
            dtDate = DateTime.Parse(strDate);
        }
        catch (FormatException eFormatException)
        {
            // the Parse method failed => the string strDate cannot be converted to a date.
            bValid = false;
        }
        return bValid;
    }
    public static bool wpm_IsNowLeapYear()
    {
        int lLngYear = Thread.CurrentThread.CurrentCulture.Calendar.GetYear(DateTime.Now);
        if (lLngYear % 4 == 0 & lLngYear % 100 != 0 | lLngYear % 400 == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    } // IsLeap()
    // ****************************************************************************
    public static string wpm_LeadingZero(string pStrValue)
    {
        if (pStrValue.Length < 2)
            pStrValue = $"0{pStrValue}";
        return pStrValue;
    }
    public static string wpm_LeadingZero(ref int pStrValue)
    {
        if (pStrValue.ToString().Length < 2)
        {
            return $"0{pStrValue}";
        }
        else
        {
            return pStrValue.ToString();
        }
    } // LeadingZero(ByRef pStrValue)

    /// <summary>
    /// Get substring of specified number of characters on the right.
    /// </summary>
    public static string Right(this string value, int length)
    {
        return value.Substring(value.Length - length);
    }

    public static string wpm_OrdinalSuffix()
    {
        int lLngDay = Thread.CurrentThread.CurrentCulture.Calendar.GetDayOfMonth(DateTime.Now);
        if (lLngDay.ToString().Right(1) == "1")
        {
            return "st";
        }
        if (lLngDay.ToString().Right(1) == "2")
            return "nd";
        if (lLngDay.ToString().Right(1) == "3")
            return "rd";

        return "th";
    } // OrdinalSuffix()
    public static string wpm_RemoveHtml(string sContent)
    {
        return Regex.Replace(sContent, "<[^>]*>", string.Empty);
    }
    public static string wpm_RemoveInvalidCharacters(string Value)
    {
        if (Value is null)
        {
            Value = string.Empty;
        }
        else
        {
            Value = Value.Trim().ToLower();
            Value = Value.Replace(" & ", string.Empty);
            Value = Value.Replace("&", string.Empty);
            Value = Value.Replace(" | ", string.Empty);
            Value = Value.Replace("|", string.Empty);
            Value = Value.Replace(",", string.Empty);
            Value = Value.Replace("/", string.Empty);
            Value = Value.Replace(@"\", string.Empty);
            Value = Value.Replace("<", string.Empty);
            Value = Value.Replace(">", string.Empty);
            Value = Value.Replace("(", string.Empty);
            Value = Value.Replace(")", string.Empty);
            Value = Value.Replace("{", string.Empty);
            Value = Value.Replace("}", string.Empty);
            Value = Value.Replace("[", string.Empty);
            Value = Value.Replace("]", string.Empty);
            Value = Value.Replace(",", string.Empty);
            Value = Value.Replace("'", string.Empty);
            Value = Value.Replace(";", string.Empty);
            Value = Value.Replace(":", string.Empty);
            Value = Value.Replace("-", string.Empty);
            Value = Value.Replace(".", string.Empty);
            Value = Value.Replace("http", string.Empty);
            Value = Value.Replace(" ", string.Empty);
            Value = Value.Replace("?", string.Empty);
            Value = Value.Replace("%22", string.Empty);
            Value = Value.Replace("=", string.Empty);
        }
        return Value;
    }
    public static string wpm_RemoveTags(string sContent)
    {
        return Regex.Replace(sContent, @"~~(.|\n)+?~~", string.Empty);
    }

}