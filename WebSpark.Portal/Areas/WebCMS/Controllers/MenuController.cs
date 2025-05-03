using System;
using System.Linq;
using WebSpark.Core.Extensions;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Core.Models.EditModels;

namespace WebSpark.Portal.Areas.WebCMS.Controllers
{
    /// <summary>
    /// Controller for managing menu items across multiple domains
    /// </summary>
    public class MenuController : WebCMSBaseController
    {
        private readonly ILogger<MenuController> _logger;
        private readonly IMenuService _menuService;
        private readonly IScopeInformation _scopeInfo;

        public MenuController(
            ILogger<MenuController> logger,
            IScopeInformation scopeInfo,
            IMenuService menuService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeInfo = scopeInfo ?? throw new ArgumentNullException(nameof(scopeInfo));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        }

        /// <summary>
        /// Displays all menu items with filtering capabilities
        /// </summary>
        /// <returns>Index view with menu items</returns>
        public IActionResult Index()
        {
            try
            {
                var menuItems = _menuService.GetAllMenuItems();
                return View(menuItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu items");
                TempData["ErrorMessage"] = "An error occurred while loading menu items.";
                return View(Enumerable.Empty<MenuModel>());
            }
        }

        /// <summary>
        /// Displays details for a specific menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Details view for the specified menu item</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid menu item ID");
                }

                var menuItem = await _menuService.GetMenuEditAsync(id);
                if (menuItem == null)
                {
                    return NotFound($"Menu item with ID {id} was not found");
                }

                return View(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu item details for ID {MenuItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading menu item details.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the menu item creation form
        /// </summary>
        /// <returns>Create view with empty menu model</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var menuItem = await _menuService.GetMenuEditAsync(0);
                return View(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing create menu form");
                TempData["ErrorMessage"] = "An error occurred while preparing the menu creation form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the creation of a new menu item
        /// </summary>
        /// <param name="item">Menu item data from form</param>
        /// <returns>Redirect to Index on success, Create view with errors otherwise</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MenuEditModel item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(item);
                }

                var menuToCreate = new MenuModel
                {
                    DomainID = item.DomainID,
                    Title = item.Title,
                    Icon = item.Icon,
                    PageContent = item.PageContent,
                    Action = item.Action,
                    ApiUrl = item.ApiUrl,
                    Argument = item.Argument,
                    Controller = item.Controller,
                    Description = item.Description,
                    DisplayInNavigation = item.DisplayInNavigation,
                    DisplayOrder = item.DisplayOrder,
                    ParentId = item.ParentId
                };

                // Auto-generate URL if not provided
                if (string.IsNullOrWhiteSpace(item.Url))
                {
                    menuToCreate.Url = item.Title.ToSlug();
                }
                else
                {
                    menuToCreate.Url = item.Url;
                }

                var saveResult = _menuService.Save(menuToCreate);
                if (saveResult == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to save the menu item");
                    return View(item);
                }

                TempData["SuccessMessage"] = $"Menu item '{item.Title}' was created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new menu item");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the menu item");
                return View(item);
            }
        }

        /// <summary>
        /// Displays the edit form for a specific menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Edit view with menu item data</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid menu item ID");
                }

                var menuItem = await _menuService.GetMenuEditAsync(id);
                if (menuItem == null)
                {
                    return NotFound($"Menu item with ID {id} was not found");
                }

                return View(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu item for editing, ID: {MenuItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while preparing the menu item for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the update of an existing menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <param name="item">Updated menu item data from form</param>
        /// <returns>Redirect to Index on success, Edit view with errors otherwise</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, MenuEditModel item)
        {
            try
            {
                if (id != item.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return View(item);
                }

                var menuToUpdate = _menuService.GetAllMenuItems().FirstOrDefault(w => w.Id == id);
                if (menuToUpdate == null)
                {
                    return NotFound($"Menu item with ID {id} was not found");
                }

                // Handle URL/Slug generation for Pages
                if (item.Controller == "Page")
                {
                    if (item.ParentId == null)
                    {
                        item.Url = item.Title.ToSlug();
                        item.Argument = item.Title.ToSlug();
                    }
                    else
                    {
                        if (item.ParentId == menuToUpdate.ParentId)
                        {
                            item.Url = $"{menuToUpdate.ParentTitle.ToSlug()}/{item.Title.ToSlug()}";
                            item.Argument = item.Url;
                        }
                    }
                }

                // Update properties
                menuToUpdate.Title = item.Title ?? menuToUpdate.Title;
                menuToUpdate.Icon = item.Icon ?? menuToUpdate.Icon;
                menuToUpdate.PageContent = item.PageContent ?? menuToUpdate.PageContent;
                menuToUpdate.Description = item.Description ?? menuToUpdate.Description;
                menuToUpdate.DisplayInNavigation = item.DisplayInNavigation;
                menuToUpdate.DisplayOrder = item.DisplayOrder;
                menuToUpdate.ParentId = item.ParentId;
                menuToUpdate.Url = item.Url;
                menuToUpdate.Controller = item.Controller ?? menuToUpdate.Controller;
                menuToUpdate.Action = item.Action ?? menuToUpdate.Action;
                menuToUpdate.Argument = item.Argument ?? menuToUpdate.Argument;

                var saveResult = _menuService.Save(menuToUpdate);
                if (saveResult == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update the menu item");
                    return View(item);
                }

                TempData["SuccessMessage"] = $"Menu item '{item.Title}' was updated successfully";
                return RedirectToAction("Details", "Website", new { id = item.DomainID, area = "WebCMS" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item ID: {MenuItemId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the menu item");
                return View(item);
            }
        }

        /// <summary>
        /// Displays the deletion confirmation page for a menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Delete view with menu item details</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid menu item ID");
                }

                var menuItem = await _menuService.GetMenuEditAsync(id);
                if (menuItem == null)
                {
                    return NotFound($"Menu item with ID {id} was not found");
                }

                return View(menuItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu item for deletion, ID: {MenuItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while preparing the menu item for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the deletion of a menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Redirect to Index on success</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid menu item ID");
                }

                var deleteResult = await _menuService.DeleteMenuAsync(id);
                if (!deleteResult)
                {
                    TempData["ErrorMessage"] = "Failed to delete the menu item";
                }
                else
                {
                    TempData["SuccessMessage"] = "Menu item was deleted successfully";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu item ID: {MenuItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the menu item";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}