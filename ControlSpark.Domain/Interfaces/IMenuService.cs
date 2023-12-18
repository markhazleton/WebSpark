﻿using ControlSpark.Domain.EditModels;
using ControlSpark.Domain.Models;

namespace ControlSpark.Domain.Interfaces;
/// <summary>
/// 
/// </summary>
public interface IMenuService
{
    IEnumerable<MenuModel> GetAllMenuItems();
    List<MenuModel> Save(List<MenuModel> saveMenus);
    MenuModel Save(MenuModel saveItem);
    List<MenuModel> GetSiteMenu(int DomainId);
    Task<MenuEditModel> GetMenuEditAsync(int Id);
    Task<bool> DeleteMenuAsync(int Id);

}
