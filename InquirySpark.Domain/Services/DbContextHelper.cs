using InquirySpark.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace InquirySpark.Domain.Services;

public static class DbContextHelper
{
    public static async Task<BaseResponse<T>> ExecuteAsync<T>(Func<Task<T?>> dbOperation)
    {
        try
        {
            var result = await dbOperation();
            return new BaseResponse<T>(result);
        }
        catch (DbUpdateException ex) // Specific exception for EF Core
        {
            return new BaseResponse<T>(["Database update error occurred.", ex.Message]);

        }
        catch (DbException ex) // General database exception
        {
            return new BaseResponse<T>(["Database error occurred.", ex.Message]);
        }
        catch (Exception ex) // General catch-all for other exceptions
        {
            return new BaseResponse<T>(["An unexpected error occurred.", ex.Message]);
        }
    }
    public static async Task<BaseResponseCollection<T>> ExecuteCollectionAsync<T>(Func<Task<List<T>>> dbOperation)
    {
        try
        {
            var result = await dbOperation();
            return new BaseResponseCollection<T>(result);
        }
        catch (DbUpdateException ex) // Specific exception for EF Core
        {
            return new BaseResponseCollection<T>(["Database update error occurred.", ex.Message]);

        }
        catch (DbException ex) // General database exception
        {
            return new BaseResponseCollection<T>(["Database error occurred.", ex.Message]);
        }
        catch (Exception ex) // General catch-all for other exceptions
        {
            return new BaseResponseCollection<T>(["An unexpected error occurred.", ex.Message]);
        }
    }
}
