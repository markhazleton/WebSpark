namespace WebSpark.Core.Interfaces;

/// <summary>
/// Interface IMenuProvider
/// </summary>
public interface IMenuProvider
{
    /// <summary>
    /// Deletes the specified identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    bool Delete(int Id);
    /// <summary>
    /// Get List of Menu Items
    /// </summary>
    /// <returns></returns>
    IEnumerable<Models.MenuModel> GetMenuList();
    /// <summary>
    /// Gets the menu by identifier.
    /// </summary>
    /// <param name="Id">The identifier.</param>
    /// <returns>MenuModel.</returns>
    Models.MenuModel GetMenuItem(int Id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    Task<Models.MenuModel> GetMenuItemAsync(int Id);
    /// <summary>
    /// Gets the site menu.
    /// </summary>
    /// <param name="DomainId">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    List<Models.MenuModel> GetSiteMenu(int DomainId);

}
