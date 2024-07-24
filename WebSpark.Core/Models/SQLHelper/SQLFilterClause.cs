namespace WebSpark.Core.Models.SQLHelper;

public class SQLFilterClause
{
    public SQLFilterClause()
    {

    }
    public SQLFilterClause(string myField, SQLFilterOperator myOperator, string myArgument, SQLFilterConjunction myConjunction, string myFieldType)
    {
        Field = myField;
        FieldOperator = myOperator;
        Argument = myArgument;
        Conjunction = myConjunction;
        FieldType = myFieldType;
    }

    public string Argument { get; set; }
    public string ClauseConjunction
    {
        get
        {
            switch (Conjunction)
            {
                case SQLFilterConjunction.andConjunction:
                    {
                        return " and ";
                    }
                case SQLFilterConjunction.orConjunction:
                    {
                        return " or ";
                    }

                default:
                    {
                        return " and ";
                    }
            }
        }
    }
    public SQLFilterConjunction Conjunction { get; set; }
    public string Field { get; set; }
    public SQLFilterOperator FieldOperator { get; set; }
    public string FieldType { get; set; }
    public string Statement
    {
        get
        {
            if (string.IsNullOrEmpty(Field))
            {
                return Argument;
            }
            else
            {
                switch (FieldOperator)
                {
                    case SQLFilterOperator.Equal:
                        {
                            if (int.TryParse(Argument, out int i))
                            {
                                return $" {Field} {" = "} {Argument} ";
                            }
                            else
                            {
                                return $" {Field}.Contains(\"{Argument}\") ";
                            }
                        }
                    case SQLFilterOperator.NotEqual:
                        {
                            return $" {Field} {" <> "} '{Argument}' ";
                        }
                    case SQLFilterOperator.LessThanEqual:
                        {
                            return $" {Field} {" <= "} '{Argument}' ";
                        }
                    case SQLFilterOperator.LessThan:
                        {
                            return $" {Field} {" < "} '{Argument}' ";
                        }
                    case SQLFilterOperator.GreaterThanEqual:
                        {
                            return $" {Field} {" >= "} '{Argument}' ";
                        }
                    case SQLFilterOperator.GreaterThan:
                        {
                            return $" {Field} {" > "} '{Argument}' ";
                        }
                    case SQLFilterOperator.dbLike:
                        {
                            return $" {Field} {" like "} '{Argument}' ";
                        }
                    case SQLFilterOperator.dbIn:
                        {
                            return $" {Field} {" in "} ({Argument}) ";
                        }
                    case SQLFilterOperator.dbBetween:
                        {
                            return $" {Field} {" between "} {Argument} ";
                        }
                    case SQLFilterOperator.dbIsNotNull:
                        {
                            return $" {Field} is not null ";
                        }
                    case SQLFilterOperator.dbIsNull:
                        {
                            return $" {Field} is null ";
                        }

                    default:
                        {
                            return $" {Field} {" = "} '{Argument}' ";
                        }
                }
            }
        }
    }
}