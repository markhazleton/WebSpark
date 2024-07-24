using WebSpark.Core.Models.EditModels;
using WebSpark.Core.Models.ViewModels;

namespace WebSpark.Core.Interfaces;


/// <summary>
/// Website Service
/// </summary>
public interface IWebsiteService
{
    /// <summary>
    /// Delete Website
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    bool Delete(int Id);
    /// <summary>
    /// Get Website List
    /// </summary>
    /// <returns></returns>
    List<Models.WebsiteModel> Get();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Models.WebsiteModel> GetAsync(int id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<WebsiteEditModel> GetEditAsync(int id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<WebsiteVM> GetBaseViewModelAsync(int id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="host"></param>
    /// <param name="defaultSiteId"></param>
    /// <returns></returns>
    Task<WebsiteVM> GetBaseViewByHostAsync(string host, string? defaultSiteId = null);

    /// <summary>
    /// Save Website
    /// </summary>
    /// <param name="saveItem"></param>
    /// <returns></returns>
    Models.WebsiteModel Save(Models.WebsiteModel saveItem);
}
