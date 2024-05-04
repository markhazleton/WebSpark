
namespace WebSpark.Domain.Models.SQLHelper;

public enum SQLFilterOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanEqual,
    LessThanEqual,
    dbLike,
    dbIn,
    dbBetween,
    dbIsNull,
    dbIsNotNull
}