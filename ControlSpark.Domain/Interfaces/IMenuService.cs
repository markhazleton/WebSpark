using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.Interfaces;
/// <summary>
/// 
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerable<MenuModel> GetAllMenuItems();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveMenus"></param>
    /// <returns></returns>
    List<MenuModel> Save(List<MenuModel> saveMenus);

    /// <summary>
    /// Saves the specified save item.
    /// </summary>
    /// <param name="saveItem">The save item.</param>
    /// <returns>MenuModel.</returns>
    MenuModel Save(MenuModel saveItem);
    /// <summary>
    /// Gets the site menu.
    /// </summary>
    /// <param name="DomainId">The domain identifier.</param>
    /// <returns>List&lt;MenuModel&gt;.</returns>
    List<MenuModel> GetSiteMenu(int DomainId);

}
