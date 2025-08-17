using System.Text;

namespace WebSpark.Core.Models.SQLHelper;

public class SQLFilterList : List<SQLHelper.SQLFilterClause>
{
    public SQLFilterList()
    {
    }
    public SQLFilterList(int capacity) : base(capacity)
    {
    }
    public SQLFilterList(IEnumerable<SQLHelper.SQLFilterClause> collection) : base(collection)
    {
    }
    private string SearchField = string.Empty;
    private bool FindClauseByField(SQLHelper.SQLFilterClause FilterClause)
    {
        if ((FilterClause.Field ?? string.Empty) == (SearchField ?? string.Empty))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public List<SQLHelper.SQLFilterClause> FindField(string reqSearchField)
    {
        SearchField = reqSearchField ?? string.Empty;
        return FindAll(FindClauseByField);
    }

    public string GetWhereClause()
    {
        var myReturn = new StringBuilder(string.Empty);
        int iLoopIndex;
        if (Count == 1)
        {
            myReturn.Append(string.Format("where {0} {1}", this[0].Statement, Environment.NewLine));
        }
        else if (Count > 1)
        {
            var loopTo = Count - 1;
            for (iLoopIndex = 0; iLoopIndex <= loopTo; iLoopIndex++)
            {
                if (iLoopIndex == 0)
                {
                    myReturn.Append(string.Format("where {0} {1}", this[iLoopIndex].Statement, Environment.NewLine));
                }
                else
                {
                    myReturn.Append(string.Format("{1}  {0} {2}", this[iLoopIndex].Statement, this[iLoopIndex].ClauseConjunction, Environment.NewLine));
                }
            }
        }
        return myReturn.ToString();
    }

    public string GetLINQWhere()
    {
        var myReturn = new StringBuilder(string.Empty);
        int iLoopIndex;
        if (Count == 1)
        {
            myReturn.Append(string.Format("{0} {1}", this[0].Statement, string.Empty));
        }
        else if (Count > 1)
        {
            var loopTo = Count - 1;
            for (iLoopIndex = 0; iLoopIndex <= loopTo; iLoopIndex++)
            {
                if (iLoopIndex == 0)
                {
                    myReturn.Append(string.Format(" {0} {1}", this[iLoopIndex].Statement, string.Empty));
                }
                else
                {
                    myReturn.Append(string.Format("{1}  {0} {2}", this[iLoopIndex].Statement, this[iLoopIndex].ClauseConjunction, string.Empty));
                }
            }
        }
        return myReturn.ToString();
    }

    public string GetWhereClause(string FieldType)
    {
        var myReturn = new StringBuilder(string.Empty);
        int iClauseCount = 0;
        int iLoopIndex;
        if (Count == 1)
        {
            if ((this[0].FieldType ?? string.Empty) == (FieldType ?? string.Empty))
            {
                myReturn.Append(string.Format("where {0} {1}", this[0].Statement, Environment.NewLine));
                iClauseCount = iClauseCount + 1;
            }
        }
        else if (Count > 1)
        {
            var loopTo = Count - 1;
            for (iLoopIndex = 0; iLoopIndex <= loopTo; iLoopIndex++)
            {
                if ((this[iLoopIndex].FieldType ?? string.Empty) == (FieldType ?? string.Empty))
                {
                    if (iClauseCount == 0)
                    {
                        myReturn.Append(string.Format("where {0} {1}", this[iLoopIndex].Statement, Environment.NewLine));
                        iClauseCount = iClauseCount + 1;
                    }
                    else
                    {
                        myReturn.Append(string.Format("{1}  {0} {2}", this[iLoopIndex].Statement, this[iLoopIndex].ClauseConjunction, Environment.NewLine));
                        iClauseCount = iClauseCount + 1;
                    }
                }
            }
        }
        return myReturn.ToString();
    }

}