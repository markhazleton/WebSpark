using System;
using System.Linq;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Core.Models.EditModels;

namespace WebSpark.Portal.Areas.WebCMS.Controllers
{
    /// <summary>
    /// Controller for managing websites and their associated menu items
    /// </summary>
    public class WebsiteController : WebCMSBaseController
    {
        private readonly IWebsiteService _service;
        private readonly ILogger<WebsiteController> _logger;
        private readonly IScopeInformation _scopeInfo;
        private readonly IMenuService _menuService;

        /// <summary>
        /// Website Controller Constructor
        /// </summary>
        public WebsiteController(
            IWebsiteService service,
            ILogger<WebsiteController> logger,
            IScopeInformation scopeInfo,
            IMenuService menuService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeInfo = scopeInfo ?? throw new ArgumentNullException(nameof(scopeInfo));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
        }

        /// <summary>
        /// Displays a list of all websites with filtering capabilities
        /// </summary>
        /// <returns>Index view with websites</returns>
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var websites = _service.Get();
                return View(websites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving websites");
                TempData["ErrorMessage"] = "An error occurred while loading websites.";
                return View(Enumerable.Empty<WebsiteModel>());
            }
        }

        /// <summary>
        /// Displays details for a specific website, including its menu structure
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns>Details view for the specified website</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid website ID");
                }

                var website = await _service.GetEditAsync(id);
                if (website == null)
                {
                    return NotFound($"Website with ID {id} was not found");
                }

                // Get menu items for this website
                var menuItems = _menuService.GetAllMenuItems()
                    .Where(m => m.DomainID == id)
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();

                ViewData["MenuItems"] = menuItems;

                return View(website);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving website details for ID {WebsiteId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading website details.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the website creation form
        /// </summary>
        /// <returns>Edit view with empty website model in Create mode</returns>
        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                var website = new WebsiteEditModel();
                ViewData["IsCreateMode"] = true;
                return View("Edit", website);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing website creation form");
                TempData["ErrorMessage"] = "An error occurred while preparing the website creation form.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the creation of a new website
        /// </summary>
        /// <param name="model">Website data from form</param>
        /// <returns>Redirect to Index on success, Edit view with errors otherwise</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebsiteEditModel model)
        {
            ViewData["IsCreateMode"] = true;

            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                var websiteToCreate = new WebsiteModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    SiteTemplate = model.SiteTemplate,
                    SiteStyle = model.SiteStyle,
                    Message = model.Message,
                    SiteName = model.SiteName,
                    WebsiteUrl = model.WebsiteUrl,
                    WebsiteTitle = model.WebsiteTitle,
                    UseBreadCrumbURL = model.UseBreadCrumbURL,
                    IsRecipeSite = model.IsRecipeSite,
                    ModifiedID = 99 // Set appropriate user ID
                };

                var saveResult = _service.Save(websiteToCreate);
                if (saveResult == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to save the website");
                    return View("Edit", model);
                }

                TempData["SuccessMessage"] = $"Website '{model.Name}' was created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new website");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the website");
                return View("Edit", model);
            }
        }

        /// <summary>
        /// Displays the edit form for a specific website
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns>Edit view with website data</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid website ID");
                }

                var website = await _service.GetEditAsync(id);
                if (website == null)
                {
                    return NotFound($"Website with ID {id} was not found");
                }

                ViewData["IsCreateMode"] = false;
                return View(website);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving website for editing, ID: {WebsiteId}", id);
                TempData["ErrorMessage"] = "An error occurred while preparing the website for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the update of an existing website
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <param name="model">Updated website data from form</param>
        /// <returns>Redirect to Details on success, Edit view with errors otherwise</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WebsiteEditModel model)
        {
            ViewData["IsCreateMode"] = false;

            try
            {
                if (id != model.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var websiteToUpdate = await _service.GetAsync(id);
                if (websiteToUpdate == null)
                {
                    return NotFound($"Website with ID {id} was not found");
                }

                // Update properties
                websiteToUpdate.Name = model.Name ?? websiteToUpdate.Name;
                websiteToUpdate.Description = model.Description ?? websiteToUpdate.Description;
                websiteToUpdate.SiteTemplate = model.SiteTemplate ?? websiteToUpdate.SiteTemplate;
                websiteToUpdate.SiteStyle = model.SiteStyle ?? websiteToUpdate.SiteStyle;
                websiteToUpdate.Message = model.Message ?? websiteToUpdate.Message;
                websiteToUpdate.SiteName = model.SiteName ?? websiteToUpdate.SiteName;
                websiteToUpdate.WebsiteUrl = model.WebsiteUrl ?? websiteToUpdate.WebsiteUrl;
                websiteToUpdate.WebsiteTitle = model.WebsiteTitle ?? websiteToUpdate.WebsiteTitle;
                websiteToUpdate.UseBreadCrumbURL = model.UseBreadCrumbURL;
                websiteToUpdate.IsRecipeSite = model.IsRecipeSite;
                websiteToUpdate.ModifiedID = 99; // Set appropriate user ID

                var saveResult = _service.Save(websiteToUpdate);
                if (saveResult == null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update the website");
                    return View(model);
                }

                TempData["SuccessMessage"] = $"Website '{model.Name}' was updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating website ID: {WebsiteId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the website");
                return View(model);
            }
        }

        /// <summary>
        /// Displays the deletion confirmation page for a website
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns>Delete view with website details</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid website ID");
                }

                var website = await _service.GetEditAsync(id);
                if (website == null)
                {
                    return NotFound($"Website with ID {id} was not found");
                }

                // Get menu items for this website to show potential impact
                var menuItems = _menuService.GetAllMenuItems()
                    .Where(m => m.DomainID == id)
                    .ToList();

                ViewData["MenuItemCount"] = menuItems.Count;

                return View(website);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving website for deletion, ID: {WebsiteId}", id);
                TempData["ErrorMessage"] = "An error occurred while preparing the website for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Processes the deletion of a website
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns>Redirect to Index on success</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid website ID");
                }

                // Get the website first to verify it exists
                var website = await _service.GetAsync(id);
                if (website == null)
                {
                    return NotFound($"Website with ID {id} was not found");
                }

                // In a real implementation, you might want to handle associated menu items
                // e.g., either delete them or reassign them

                var deleteResult = _service.Delete(id);
                if (!deleteResult)
                {
                    TempData["ErrorMessage"] = "Failed to delete the website";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                TempData["SuccessMessage"] = "Website was deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting website ID: {WebsiteId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the website";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}