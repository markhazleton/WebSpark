using WebSpark.Domain.EditModels;
using WebSpark.Domain.Models;
using WebSpark.Domain.ViewModels;

namespace WebSpark.Domain.Interfaces;


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
    List<WebsiteModel> Get();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<WebsiteModel> GetAsync(int id);
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
    WebsiteModel Save(WebsiteModel saveItem);
}
