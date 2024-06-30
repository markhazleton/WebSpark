using System.Text;

namespace WebSpark.Domain.Models;

/// <summary>
/// Class WebDataGrid.
/// </summary>
public class WebDataGrid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebDataGrid" /> class.
    /// </summary>
    public WebDataGrid()
    {
        GridColumns = [];
        GridRows = [];
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the detail path.
    /// </summary>
    /// <value>The detail path.</value>
    public string DetailPath { get; set; }

    /// <summary>
    /// Gets or sets the name of the detail key.
    /// </summary>
    /// <value>The name of the detail key.</value>
    public string DetailKeyName { get; set; }

    /// <summary>
    /// Gets or sets the name of the detail field.
    /// </summary>
    /// <value>The name of the detail field.</value>
    public string DetailFieldName { get; set; }

    /// <summary>
    /// Gets or sets the index of the detail key grid.
    /// </summary>
    /// <value>The index of the detail key grid.</value>
    public int DetailKeyGridIndex { get; set; }

    /// <summary>
    /// Gets or sets the index of the detail field grid.
    /// </summary>
    /// <value>The index of the detail field grid.</value>
    public int DetailFieldGridIndex { get; set; }

    /// <summary>
    /// Gets or sets the display name of the detail.
    /// </summary>
    /// <value>The display name of the detail.</value>
    public string DetailDisplayName { get; set; }

    /// <summary>
    /// Gets the grid columns.
    /// </summary>
    /// <value>The grid columns.</value>
    public ColumnColl GridColumns { get; }

    /// <summary>
    /// Gets the grid rows.
    /// </summary>
    /// <value>The grid rows.</value>
    public RowColl GridRows { get; }

    /// <summary>
    /// Class GridColumn.
    /// </summary>
    public class GridColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridColumn" /> class.
        /// </summary>
        public GridColumn()
        {
            ViewOnSummary = true;
            ViewOnPhone = true;
            ColumnValues = [];
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the source.
        /// </summary>
        /// <value>The name of the source.</value>
        public string SourceName { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        public DisplayFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        public int Index { get; set; }

        // public string Value { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [key field].
        /// </summary>
        /// <value><c>true</c> if [key field]; otherwise, <c>false</c>.</value>
        public bool KeyField { get; set; }

        /// <summary>
        /// Gets or sets the link path.
        /// </summary>
        /// <value>The link path.</value>
        public string LinkPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the link key.
        /// </summary>
        /// <value>The name of the link key.</value>
        public string LinkKeyName { get; set; }

        /// <summary>
        /// Gets or sets the index of the link key.
        /// </summary>
        /// <value>The index of the link key.</value>
        public int LinkKeyIndex { get; set; }

        /// <summary>
        /// Gets or sets the name of the link text.
        /// </summary>
        /// <value>The name of the link text.</value>
        public string LinkTextName { get; set; }

        /// <summary>
        /// Gets or sets the index of the link text.
        /// </summary>
        /// <value>The index of the link text.</value>
        public int LinkTextIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [view on summary].
        /// </summary>
        /// <value><c>true</c> if [view on summary]; otherwise, <c>false</c>.</value>
        public bool ViewOnSummary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [view on phone].
        /// </summary>
        /// <value><c>true</c> if [view on phone]; otherwise, <c>false</c>.</value>
        public bool ViewOnPhone { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail path.
        /// </summary>
        /// <value>The thumbnail path.</value>
        public string ThumbnailPath { get; set; }

        //public string DataType { get; set; }
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        public string MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        public string MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the unique values.
        /// </summary>
        /// <value>The unique values.</value>
        public int UniqueValues { get; set; }

        /// <summary>
        /// Gets or sets the most common.
        /// </summary>
        /// <value>The most common.</value>
        public string MostCommon { get; set; }

        /// <summary>
        /// Gets or sets the least common.
        /// </summary>
        /// <value>The least common.</value>
        public string LeastCommon { get; set; }

        /// <summary>
        /// Gets or sets the column values.
        /// </summary>
        /// <value>The column values.</value>
        public List<string> ColumnValues { get; set; }

        /// <summary>
        /// The col dictionary
        /// </summary>
        private Dictionary<string, int> ColDictionary = [];

        /// <summary>
        /// Updates the dictionary.
        /// </summary>
        /// <param name="ColValue">The col value.</param>
        public void UpdateDictionary(string ColValue)
        {
            if (ColDictionary.ContainsKey(GetStringValue(ColValue)))
            {
                int value = 0;
                if ((ColDictionary.TryGetValue(GetStringValue(ColValue), out value)))
                {
                    ColDictionary[GetStringValue(ColValue)] = value + 1;
                }
            }
            else
            {
                ColDictionary.Add(GetStringValue(ColValue), 1);
                UniqueValues = UniqueValues + 1;
            }
        }

        /// <summary>
        /// Fixes the name of the column.
        /// </summary>
        private void FixColumnName()
        {
            DisplayName = DisplayName.Replace(" ", string.Empty);
            DisplayName = DisplayName.Replace("?", string.Empty);
        }

        /// <summary>
        /// Sets the common values.
        /// </summary>
        public void SetCommonValues()
        {
            if (ColDictionary.Count > 0)
            {
                FixColumnName();
                if (Format == DisplayFormat.Number)
                {
                    try
                    {
                        MaxValue = (from x in ColDictionary orderby Convert.ToDecimal(x.Key) descending select x.Key).FirstOrDefault();
                    }
                    catch
                    {
                    }
                    try
                    {
                        MinValue = (from x in ColDictionary orderby Convert.ToDecimal(x.Key) ascending select x.Key).FirstOrDefault();
                    }
                    catch
                    {
                    }
                }
                else
                {
                    MaxValue = (from x in ColDictionary orderby x.Key descending select x.Key).FirstOrDefault();
                    MinValue = (from x in ColDictionary orderby x.Key ascending select x.Key).FirstOrDefault();
                }
                MostCommon = (from x in ColDictionary
                              orderby x.Value ascending
                              select $"'{x.Key}' in {x.Value} rows").First();
                LeastCommon = (from x in ColDictionary
                               orderby x.Value descending
                               select $"'{x.Key}' in {x.Value} rows").First();
                ColumnValues.Clear();
                ColumnValues.AddRange((from entry in ColDictionary orderby entry.Key ascending select entry.Key).ToArray());
            }
        }

        /// <summary>
        /// Gets the format table cell.
        /// </summary>
        /// <param name="PropValue">The property value.</param>
        /// <param name="LinkURL">The link URL.</param>
        /// <returns>System.String.</returns>
        public string GetFormatTableCell(string PropValue, string LinkURL)
        {
            string myReturn = string.Empty;
            if (IsNumeric(PropValue))
            {
                switch (Format)
                {
                    case DisplayFormat.Currency:
                        PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString("c");
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
                        break;

                    case DisplayFormat.Text:
                        PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString();
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
                        break;

                    case DisplayFormat.Number:
                        PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString();
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
                        break;

                    case DisplayFormat.Percent:
                        PropValue = Math.Round(Convert.ToDouble(PropValue), 4).ToString();
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
                        break;

                    case DisplayFormat.Float:
                        PropValue = Math.Round(Convert.ToDouble(PropValue), 4).ToString("N4");
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
                        break;

                    case DisplayFormat.Thumbnail:
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' ><img width='50px' src='{LinkURL}' alt='{PropValue}' />{PropValue}</td>");
                        break;

                    default:
                        PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString();
                        myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
                        break;
                }
            }
            else
            {
                myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' ><a href='{LinkURL}' >{PropValue}</a></td>");
            }
            return myReturn;
        }

        /// <summary>
        /// Determines whether the specified property value is numeric.
        /// </summary>
        /// <param name="propValue">The property value.</param>
        /// <returns><c>true</c> if the specified property value is numeric; otherwise, <c>false</c>.</returns>
        private bool IsNumeric(string propValue)
        {
            double retNum;
            bool isNum = double.TryParse(Convert.ToString(propValue),
                                         NumberStyles.Any,
                                         NumberFormatInfo.InvariantInfo,
                                         out retNum);
            return isNum;
        }

        /// <summary>
        /// Gets the format table cell.
        /// </summary>
        /// <param name="gRow">The g row.</param>
        /// <returns>System.String.</returns>
        public string GetFormatTableCell(GridRow gRow)
        {
            string myReturn = string.Empty;
            string PropValue = gRow.Value[Index];

            if (string.IsNullOrEmpty(LinkPath))
            {
                if (IsNumeric(PropValue))
                {
                    switch (Format)
                    {
                        case DisplayFormat.ShortDate:
                            PropValue = GetShortDate(PropValue);
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' >{PropValue}</td>");
                            break;

                        case DisplayFormat.Currency:
                            PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString("c");
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' >{PropValue}</td>");
                            break;

                        case DisplayFormat.Text:
                            PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString();
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' >{PropValue}</td>");
                            break;

                        case DisplayFormat.Number:
                            PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString("N0");
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' >{PropValue}</td>");
                            break;

                        case DisplayFormat.Percent:
                            PropValue = Math.Round(Convert.ToDouble(PropValue), 4).ToString("P");
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' >{PropValue}</td>");
                            break;

                        case DisplayFormat.Float:
                            PropValue = Math.Round(Convert.ToDouble(PropValue), 4).ToString("N4");
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: right; ' >{PropValue}</td>");
                            break;

                        default:
                            PropValue = Math.Round(Convert.ToDecimal(PropValue), 2).ToString();
                            myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' >{PropValue}</td>");
                            break;
                    }
                }
                else
                {
                    myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' >{PropValue}</td>");
                }
            }
            else
            {
                string linkURL = string.Format(LinkPath, PropValue);
                string linkText = PropValue;

                if (LinkTextIndex > 0)
                {
                    linkText = gRow.Value[LinkTextIndex - 1];
                }
                if (LinkKeyIndex > 0)
                {
                    linkURL = string.Format(LinkPath, gRow.Value[LinkKeyIndex - 1]);
                }

                myReturn = ($"<td {GetTDCssClass()} style='text-align: left; ' ><a href='{linkURL}'>{linkText}</a></td>");
            }

            return myReturn;
        }

        /// <summary>
        /// Gets the short date.
        /// </summary>
        /// <param name="propValue">The property value.</param>
        /// <returns>System.String.</returns>
        private string GetShortDate(string propValue)
        {
            return DateTime.Parse(propValue).ToShortDateString();
        }

        /// <summary>
        /// Gets the td CSS class.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetTDCssClass()
        {
            if (ViewOnPhone)
            {
                return string.Empty;
            }
            else
            {
                return " class='hidden-xs' ";
            }
        }
    }

    /// <summary>
    /// Class GridRow.
    /// </summary>
    public class GridRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridRow" /> class.
        /// </summary>
        public GridRow()
        {
            Value = [];
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public List<string> Value { get; set; }
    }

    /// <summary>
    /// Class RowColl.
    /// </summary>
    public class RowColl : List<GridRow>
    {
    }

    /// <summary>
    /// Class ColumnColl.
    /// </summary>
    public class ColumnColl : List<GridColumn>
    {
        /// <summary>
        /// Adds the specified column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="displayFormat">The display format.</param>
        public void Add(string columnName, string sourceName, DisplayFormat displayFormat)
        {
            Add(new GridColumn() { Name = columnName, SourceName = sourceName, Format = displayFormat });
        }

        /// <summary>
        /// Adds the specified column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="displayFormat">The display format.</param>
        public void Add(string columnName, string sourceName, string displayName, DisplayFormat displayFormat)
        {
            Add(new GridColumn()
            { Name = columnName, SourceName = sourceName, DisplayName = displayName, Format = displayFormat });
        }

        /// <summary>
        /// Adds the specified column name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="sourceName">Name of the source.</param>
        /// <param name="displayFormat">The display format.</param>
        /// <param name="displayOnSummary">if set to <c>true</c> [display on summary].</param>
        internal void Add(string columnName, string sourceName, DisplayFormat displayFormat, bool displayOnSummary)
        {
            Add(new GridColumn()
            {
                Name = columnName,
                SourceName = sourceName,
                Format = displayFormat,
                ViewOnSummary = displayOnSummary
            });
        }
    }

    /// <summary>
    /// Gets the display table header.
    /// </summary>
    /// <returns>System.String.</returns>
    public string GetDisplayTableHeader()
    {
        StringBuilder myrow = new();
        if (DetailFieldName != string.Empty)
        {
            if (DetailDisplayName != string.Empty)
            {
                myrow.AppendLine($"<th>{FormatHeaderColumn(DetailDisplayName)}</th>");
            }
            else
            {
                myrow.AppendLine($"<th>{FormatHeaderColumn(DetailFieldName)}</th>");
            }
        }
        foreach (GridColumn gCol in GridColumns)
        {
            myrow.AppendLine($"<th {gCol.GetTDCssClass()}  >{FormatHeaderColumn(gCol.DisplayName)}</th>");
        }
        return myrow.ToString();
    }

    /// <summary>
    /// Formats the header column.
    /// </summary>
    /// <param name="myColName">Name of my col.</param>
    /// <returns>System.String.</returns>
    private string FormatHeaderColumn(string myColName)
    {
        string newColumnName = null;
        switch (StrFun.Right(myColName, 2))
        {
            case "NM":
                newColumnName = $"{(StrFun.Left(myColName, myColName.Length - 2))} Name";
                break;

            case "CD":
                newColumnName = $"{(StrFun.Left(myColName, myColName.Length - 2))} SiteTemplate";
                break;

            case "DS":
                newColumnName = $"{(StrFun.Left(myColName, myColName.Length - 2))} Description";
                break;

            case "ID":
                newColumnName = $"{(StrFun.Left(myColName, myColName.Length - 2))} Id";
                break;

            default:
                newColumnName = myColName;
                break;
        }

        string newstring = string.Empty;
        for (int i = 0; i <= newColumnName.Length - 1; i++)
        {
            if (char.IsUpper(newColumnName[i]) && i > 0)
            {
                newstring += " ";
            }
            newstring += newColumnName[i].ToString();
        }
        return newstring;
    }

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="myCol">My col.</param>
    /// <returns>System.String.</returns>
    public string GetPropertyValue(object obj, GridColumn myCol)
    {
        Type objType = default(Type);
        PropertyInfo pInfo = default(PropertyInfo);
        object PropValue = new();
        if (myCol.Name.Contains("."))
        {
            List<string> PropertyNameArray = myCol.Name.Split('.').ToList();

            if (PropertyNameArray.Count() == 2)
            {
                try
                {
                    objType = obj.GetType();
                    pInfo = objType.GetProperty(PropertyNameArray[0]);
                    PropValue = pInfo.GetValue(obj, BindingFlags.GetProperty, null, null, null);

                    objType = PropValue.GetType();
                    pInfo = objType.GetProperty(PropertyNameArray[1]);
                    PropValue = pInfo.GetValue(PropValue, BindingFlags.GetProperty, null, null, null);
                }
                catch
                {
                    PropValue = string.Empty;
                }
            }
        }
        else
        {
            try
            {
                objType = obj.GetType();
                pInfo = objType.GetProperty(myCol.Name);
                DateTime myDateValue;
                switch (myCol.Format)
                {
                    case DisplayFormat.ShortDate:
                        if (DateTime.TryParse(pInfo.GetValue(obj, BindingFlags.GetProperty, null, null, null)
                            .ToString(),
                                             out myDateValue))
                        {
                            PropValue = myDateValue.Date.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            PropValue = pInfo.GetValue(obj, BindingFlags.GetProperty, null, null, null);
                        }
                        break;

                    case DisplayFormat.LongDate:
                        if (DateTime.TryParse(pInfo.GetValue(obj, BindingFlags.GetProperty, null, null, null)
                            .ToString(),
                                             out myDateValue))
                        {
                            PropValue = myDateValue.ToString("yyyy-MM-dd:HH.mm");
                        }
                        else
                        {
                            PropValue = pInfo.GetValue(obj, BindingFlags.GetProperty, null, null, null);
                        }
                        break;

                    default:
                        PropValue = pInfo.GetValue(obj, BindingFlags.GetProperty, null, null, null);
                        break;
                }
            }
            catch
            {
                PropValue = string.Empty;
            }
            if (PropValue == null)
            {
                PropValue = string.Empty;
            }
        }

        PropValue = ClearLineFeeds(PropValue.ToString());

        return PropValue.ToString();
    }

    /// <summary>
    /// Gets the string value.
    /// </summary>
    /// <param name="myString">My string.</param>
    /// <returns>System.String.</returns>
    public static string GetStringValue(string myString)
    {
        if (string.IsNullOrEmpty(myString))
        {
            myString = string.Empty;
        }
        return myString;
    }

    /// <summary>
    /// Clears the line feeds.
    /// </summary>
    /// <param name="sTextToCovert">The s text to covert.</param>
    /// <returns>System.String.</returns>
    public static string ClearLineFeeds(string sTextToCovert)
    {
        sTextToCovert = sTextToCovert.Replace("\n", string.Empty);
        return sTextToCovert;
    }

    /// <summary>
    /// Enum DisplayFormat
    /// </summary>
    public enum DisplayFormat
    {
        /// <summary>
        /// The number
        /// </summary>
        Number,
        /// <summary>
        /// The currency
        /// </summary>
        Currency,
        /// <summary>
        /// The text
        /// </summary>
        Text,
        /// <summary>
        /// The percent
        /// </summary>
        Percent,
        /// <summary>
        /// The float
        /// </summary>
        Float,
        /// <summary>
        /// The thumbnail
        /// </summary>
        Thumbnail,
        /// <summary>
        /// The hidden
        /// </summary>
        Hidden,
        /// <summary>
        /// The short date
        /// </summary>
        ShortDate,
        /// <summary>
        /// The long date
        /// </summary>
        LongDate,
        /// <summary>
        /// The link
        /// </summary>
        Link
    }

    /// <summary>
    /// Enum ColumnFunction
    /// </summary>
    public enum ColumnFunction
    {
        /// <summary>
        /// The value
        /// </summary>
        Value,
        /// <summary>
        /// The sum
        /// </summary>
        Sum,
        /// <summary>
        /// The count
        /// </summary>
        Count,
        /// <summary>
        /// The average
        /// </summary>
        Average
    }

    /// <summary>
    /// The header items
    /// </summary>
    public List<GridColumn> HeaderItems = [];

    /// <summary>
    /// Adds the header item.
    /// </summary>
    /// <param name="DisplayName">The display name.</param>
    /// <param name="Name">The name.</param>
    public void AddHeaderItem(string DisplayName, string Name)
    {
        foreach (GridColumn x in GridColumns)
        {
            if (x.Name == Name)
            {
                x.ViewOnSummary = true;
                x.DisplayName = DisplayName;
                break;
            }
        }
    }

    /// <summary>
    /// Adds the header item.
    /// </summary>
    /// <param name="DisplayName">The display name.</param>
    /// <param name="Name">The name.</param>
    /// <param name="ShowOnPhone">if set to <c>true</c> [show on phone].</param>
    public void AddHeaderItem(string DisplayName, string Name, bool ShowOnPhone)
    {
        foreach (GridColumn x in GridColumns)
        {
            if (x.Name == Name)
            {
                x.ViewOnSummary = true;
                x.ViewOnPhone = ShowOnPhone;
                x.DisplayName = DisplayName;
                break;
            }
        }
    }

    /// <summary>
    /// Adds the header item.
    /// </summary>
    /// <param name="DisplayName">The display name.</param>
    /// <param name="Name">The name.</param>
    /// <param name="ShowOnPhone">if set to <c>true</c> [show on phone].</param>
    /// <param name="Display">The display.</param>
    public void AddHeaderItem(string DisplayName, string Name, bool ShowOnPhone, DisplayFormat Display)
    {
        foreach (GridColumn x in GridColumns)
        {
            if (x.Name == Name)
            {
                x.ViewOnSummary = true;
                x.ViewOnPhone = ShowOnPhone;
                x.DisplayName = DisplayName;
                x.Format = Display;
                break;
            }
        }
    }

    /// <summary>
    /// Adds the link header item.
    /// </summary>
    /// <param name="DisplayName">The display name.</param>
    /// <param name="Name">The name.</param>
    /// <param name="LinkPath">The link path.</param>
    /// <param name="LinkKeyName">Name of the link key.</param>
    public void AddLinkHeaderItem(string DisplayName, string Name, string LinkPath, string LinkKeyName)
    {
        foreach (GridColumn x in GridColumns)
        {
            if (x.Name == Name)
            {
                x.ViewOnSummary = true;
                x.ViewOnPhone = true;
                x.DisplayName = DisplayName;
                x.LinkKeyName = LinkKeyName;
                x.LinkPath = LinkPath;
                break;
            }
        }
    }

    /// <summary>
    /// Adds the link header item.
    /// </summary>
    /// <param name="DisplayName">The display name.</param>
    /// <param name="Name">The name.</param>
    /// <param name="LinkPath">The link path.</param>
    /// <param name="LinkKeyName">Name of the link key.</param>
    /// <param name="LinkTextName">Name of the link text.</param>
    public void AddLinkHeaderItem(string DisplayName,
                                  string Name,
                                  string LinkPath,
                                  string LinkKeyName,
                                  string LinkTextName)
    {
        foreach (GridColumn x in GridColumns)
        {
            if (x.Name == Name)
            {
                x.ViewOnSummary = true;
                x.ViewOnPhone = true;
                x.DisplayName = DisplayName;
                x.LinkKeyName = LinkKeyName;
                x.LinkPath = LinkPath;
                x.LinkTextName = LinkTextName;
                break;
            }
        }
    }

    /// <summary>
    /// Adds the link header item.
    /// </summary>
    /// <param name="DisplayName">The display name.</param>
    /// <param name="Name">The name.</param>
    /// <param name="LinkPath">The link path.</param>
    /// <param name="LinkKeyName">Name of the link key.</param>
    /// <param name="LinkTextName">Name of the link text.</param>
    /// <param name="ThumbnailPath">The thumbnail path.</param>
    public void AddLinkHeaderItem(string DisplayName,
                                  string Name,
                                  string LinkPath,
                                  string LinkKeyName,
                                  string LinkTextName,
                                  string ThumbnailPath)
    {
        foreach (GridColumn x in GridColumns)
        {
            if (x.Name == Name)
            {
                x.ViewOnSummary = true;
                x.ViewOnPhone = true;
                x.DisplayName = DisplayName;
                x.LinkKeyName = LinkKeyName;
                x.LinkPath = LinkPath;
                x.LinkTextName = LinkTextName;
                x.ThumbnailPath = ThumbnailPath;
                break;
            }
        }
    }

    /// <summary>
    /// Gets the CSV.
    /// </summary>
    /// <returns>System.String.</returns>
    public string GetCSV()
    {
        StringBuilder csv = new();
        foreach (GridColumn column in GridColumns)
        {
            //Add the Header row for CSV file.
            csv.Append($"{(string.Concat("\"", column.DisplayName.ToString().Replace("\"", "\"\""), "\""))},");
        }
        //Add new line.
        csv.Append(Environment.NewLine);
        foreach (GridRow row in GridRows)
        {
            foreach (GridColumn column in GridColumns)
            {
                //Add the Data rows.
                csv.Append($"{(QuoteCSVValue(column, row.Value[column.Index]))},");
            }
            //Add new line.
            csv.Append(Environment.NewLine);
        }
        return csv.ToString();
    }

    /// <summary>
    /// Quotes the CSV value.
    /// </summary>
    /// <param name="col">The col.</param>
    /// <param name="value">The value.</param>
    /// <returns>System.String.</returns>
    private static string QuoteCSVValue(GridColumn col, object value)
    {
        var valType = value.GetType();
        switch (col.Format)
        {
            case DisplayFormat.Currency:
                return value.ToString();

            case DisplayFormat.Float:
                return value.ToString();

            case DisplayFormat.Percent:
                return value.ToString();

            case DisplayFormat.Number:
                return value.ToString();

            case DisplayFormat.ShortDate:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");
            //if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
            //{
            //    return String.Concat("\"", ((DateTime)value).ToShortDateString().Replace("\"", "\"\""), "\"");
            //}
            //else
            //{
            //    return String.Concat("\"", ((DateTime)value).ToString().Replace("\"", "\"\""), "\"");
            //}
            case DisplayFormat.LongDate:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");
            //if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
            //{
            //    return String.Concat("\"", ((DateTime)value).ToShortDateString().Replace("\"", "\"\""), "\"");
            //}
            //else
            //{
            //    return String.Concat("\"", ((DateTime)value).ToString().Replace("\"", "\"\""), "\"");
            //}
            case DisplayFormat.Thumbnail:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");

            case DisplayFormat.Link:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");

            case DisplayFormat.Hidden:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");

            case DisplayFormat.Text:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");

            default:
                return string.Concat("\"", value.ToString().Replace("\"", "\"\""), "\"");
        }
    }

    /// <summary>
    /// Determines whether the specified expression is numeric.
    /// </summary>
    /// <param name="Expression">The expression.</param>
    /// <returns><c>true</c> if the specified expression is numeric; otherwise, <c>false</c>.</returns>
    public static bool IsNumeric(object Expression)
    {
        double retNum;

        bool isNum = double.TryParse(Convert.ToString(Expression),
                                     NumberStyles.Any,
                                     NumberFormatInfo.InvariantInfo,
                                     out retNum);
        return isNum;
    }

    /// <summary>
    /// Class StrFun.
    /// </summary>
    private class StrFun
    {
        /// <summary>
        /// Lefts the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.String.</returns>
        public static string Left(string param, int length)
        {
            //we start at 0 since we want to get the characters starting from the
            //left and with the specified length and assign it to a variable
            string result = param.Substring(0, length);
            //return the result of the operation
            return result;
        }

        /// <summary>
        /// Rights the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.String.</returns>
        public static string Right(string param, int length)
        {
            //start at the index based on the length of the sting minus
            //the specified length and assign it a variable
            string result = param.Substring(param.Length - length, length);
            //return the result of the operation
            return result;
        }

        /// <summary>
        /// Mids the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>System.String.</returns>
        public static string Mid(string param, int startIndex, int length)
        {
            //start at the specified index in the string ang get N number of
            //characters depending on the length and assign it to a variable
            string result = param.Substring(startIndex, length);
            //return the result of the operation
            return result;
        }

        /// <summary>
        /// Mids the specified parameter.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns>System.String.</returns>
        public static string Mid(string param, int startIndex)
        {
            //start at the specified index and return all characters after it
            //and assign it to a variable
            string result = param.Substring(startIndex);
            //return the result of the operation
            return result;
        }
    }
}
