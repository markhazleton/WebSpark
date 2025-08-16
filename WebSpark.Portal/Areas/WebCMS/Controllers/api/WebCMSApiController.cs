using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebSpark.Core.Interfaces;
using WebSpark.Core.Models;
using WebSpark.Core.Models.EditModels;
using WebSpark.Core.Extensions;
using System.ComponentModel.DataAnnotations;

namespace WebSpark.Portal.Areas.WebCMS.Controllers.Api
{
    /// <summary>
    /// RESTful API controller for WebCMS operations covering all entities with full CRUD support
    /// </summary>
    [ApiController]
    [Area("WebCMS")]
    [Route("api/[area]")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class WebCMSApiController : ControllerBase
    {
        private readonly ILogger<WebCMSApiController> _logger;
        private readonly IWebsiteService _websiteService;
        private readonly IMenuService _menuService;
        private readonly IScopeInformation _scopeInfo;

        public WebCMSApiController(
            ILogger<WebCMSApiController> logger,
            IWebsiteService websiteService,
            IMenuService menuService,
            IScopeInformation scopeInfo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _websiteService = websiteService ?? throw new ArgumentNullException(nameof(websiteService));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _scopeInfo = scopeInfo ?? throw new ArgumentNullException(nameof(scopeInfo));
        }

        #region Website CRUD Operations

        /// <summary>
        /// Get all websites with optional filtering and pagination
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page (max 100)</param>
        /// <param name="search">Search term for website name or description</param>
        /// <param name="template">Filter by template type</param>
        /// <param name="isRecipeSite">Filter by recipe site flag</param>
        /// <returns>Paginated list of websites</returns>
        [HttpGet("websites")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<WebsiteModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<WebsiteModel>>>> GetWebsites(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? template = null,
            [FromQuery] bool? isRecipeSite = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var websites = _websiteService.Get().AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(search))
                {
                    websites = websites.Where(w =>
                        w.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(w.Description) && w.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(template))
                {
                    websites = websites.Where(w =>
                        string.Equals(w.SiteTemplate, template, StringComparison.OrdinalIgnoreCase));
                }

                if (isRecipeSite.HasValue)
                {
                    websites = websites.Where(w => w.IsRecipeSite == isRecipeSite.Value);
                }

                var totalCount = websites.Count();
                var pagedWebsites = websites
                    .OrderBy(w => w.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PaginatedResult<WebsiteModel>
                {
                    Items = pagedWebsites,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(new ApiResponse<PaginatedResult<WebsiteModel>>
                {
                    Success = true,
                    Data = result,
                    Message = $"Retrieved {pagedWebsites.Count} websites"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving websites");
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving websites"
                });
            }
        }

        /// <summary>
        /// Get a specific website by ID
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns>Website details</returns>
        [HttpGet("websites/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<WebsiteModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<WebsiteModel>>> GetWebsite(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid ID",
                        Message = "Website ID must be greater than 0"
                    });
                }

                var website = await _websiteService.GetAsync(id);

                if (website == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Website not found",
                        Message = $"Website with ID {id} was not found"
                    });
                }

                return Ok(new ApiResponse<WebsiteModel>
                {
                    Success = true,
                    Data = website,
                    Message = "Website retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving website with ID {WebsiteId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving the website"
                });
            }
        }

        /// <summary>
        /// Create a new website
        /// </summary>
        /// <param name="request">Website creation request</param>
        /// <returns>Created website</returns>
        [HttpPost("websites")]
        [ProducesResponseType(typeof(ApiResponse<WebsiteModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<WebsiteModel>>> CreateWebsite([FromBody] CreateWebsiteRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Validation failed",
                        Message = "Invalid website data provided",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var website = new WebsiteModel
                {
                    Name = request.Name,
                    Description = request.Description,
                    SiteTemplate = request.SiteTemplate,
                    SiteStyle = request.SiteStyle,
                    Message = request.Message,
                    SiteName = request.SiteName,
                    WebsiteUrl = request.WebsiteUrl,
                    WebsiteTitle = request.WebsiteTitle,
                    UseBreadCrumbURL = request.UseBreadCrumbURL,
                    IsRecipeSite = request.IsRecipeSite,
                    ModifiedID = 99 // TODO: Set from current user context
                };

                var createdWebsite = _websiteService.Save(website);

                if (createdWebsite == null)
                {
                    return StatusCode(500, new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Creation failed",
                        Message = "Failed to create the website"
                    });
                }

                return CreatedAtAction(
                    nameof(GetWebsite),
                    new { id = createdWebsite.Id },
                    new ApiResponse<WebsiteModel>
                    {
                        Success = true,
                        Data = createdWebsite,
                        Message = "Website created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating website");
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while creating the website"
                });
            }
        }

        /// <summary>
        /// Update an existing website
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <param name="request">Website update request</param>
        /// <returns>Updated website</returns>
        [HttpPut("websites/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<WebsiteModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<WebsiteModel>>> UpdateWebsite(int id, [FromBody] UpdateWebsiteRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid ID",
                        Message = "Website ID must be greater than 0"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Validation failed",
                        Message = "Invalid website data provided",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var existingWebsite = await _websiteService.GetAsync(id);
                if (existingWebsite == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Website not found",
                        Message = $"Website with ID {id} was not found"
                    });
                }

                // Update properties
                existingWebsite.Name = request.Name ?? existingWebsite.Name;
                existingWebsite.Description = request.Description ?? existingWebsite.Description;
                existingWebsite.SiteTemplate = request.SiteTemplate ?? existingWebsite.SiteTemplate;
                existingWebsite.SiteStyle = request.SiteStyle ?? existingWebsite.SiteStyle;
                existingWebsite.Message = request.Message ?? existingWebsite.Message;
                existingWebsite.SiteName = request.SiteName ?? existingWebsite.SiteName;
                existingWebsite.WebsiteUrl = request.WebsiteUrl ?? existingWebsite.WebsiteUrl;
                existingWebsite.WebsiteTitle = request.WebsiteTitle ?? existingWebsite.WebsiteTitle;
                existingWebsite.UseBreadCrumbURL = request.UseBreadCrumbURL;
                existingWebsite.IsRecipeSite = request.IsRecipeSite;
                existingWebsite.ModifiedID = 99; // TODO: Set from current user context

                var updatedWebsite = _websiteService.Save(existingWebsite);

                if (updatedWebsite == null)
                {
                    return StatusCode(500, new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Update failed",
                        Message = "Failed to update the website"
                    });
                }

                return Ok(new ApiResponse<WebsiteModel>
                {
                    Success = true,
                    Data = updatedWebsite,
                    Message = "Website updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating website with ID {WebsiteId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while updating the website"
                });
            }
        }

        /// <summary>
        /// Delete a website
        /// </summary>
        /// <param name="id">Website ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("websites/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteWebsite(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid ID",
                        Message = "Website ID must be greater than 0"
                    });
                }

                var website = await _websiteService.GetAsync(id);
                if (website == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Website not found",
                        Message = $"Website with ID {id} was not found"
                    });
                }

                var deleteResult = _websiteService.Delete(id);

                if (!deleteResult)
                {
                    return StatusCode(500, new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Deletion failed",
                        Message = "Failed to delete the website"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { deletedId = id },
                    Message = "Website deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting website with ID {WebsiteId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while deleting the website"
                });
            }
        }

        #endregion

        #region Menu CRUD Operations

        /// <summary>
        /// Get all menu items with optional filtering and pagination
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page (max 100)</param>
        /// <param name="search">Search term for menu title or description</param>
        /// <param name="domainId">Filter by domain/website ID</param>
        /// <param name="parentId">Filter by parent menu ID</param>
        /// <param name="displayInNavigation">Filter by navigation visibility</param>
        /// <returns>Paginated list of menu items</returns>
        [HttpGet("menus")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<MenuModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<MenuModel>>>> GetMenuItems(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? domainId = null,
            [FromQuery] int? parentId = null,
            [FromQuery] bool? displayInNavigation = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var menuItems = _menuService.GetAllMenuItems().AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(search))
                {
                    menuItems = menuItems.Where(m =>
                        m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(m.Description) && m.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
                }

                if (domainId.HasValue)
                {
                    menuItems = menuItems.Where(m => m.DomainID == domainId.Value);
                }

                if (parentId.HasValue)
                {
                    menuItems = menuItems.Where(m => m.ParentId == parentId.Value);
                }

                if (displayInNavigation.HasValue)
                {
                    menuItems = menuItems.Where(m => m.DisplayInNavigation == displayInNavigation.Value);
                }

                var totalCount = menuItems.Count();
                var pagedMenuItems = menuItems
                    .OrderBy(m => m.DisplayOrder)
                    .ThenBy(m => m.Title)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PaginatedResult<MenuModel>
                {
                    Items = pagedMenuItems,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(new ApiResponse<PaginatedResult<MenuModel>>
                {
                    Success = true,
                    Data = result,
                    Message = $"Retrieved {pagedMenuItems.Count} menu items"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu items");
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving menu items"
                });
            }
        }

        /// <summary>
        /// Get a specific menu item by ID
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Menu item details</returns>
        [HttpGet("menus/{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<MenuEditModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MenuEditModel>>> GetMenuItem(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid ID",
                        Message = "Menu item ID must be greater than 0"
                    });
                }

                var menuItem = await _menuService.GetMenuEditAsync(id);

                if (menuItem == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Menu item not found",
                        Message = $"Menu item with ID {id} was not found"
                    });
                }

                return Ok(new ApiResponse<MenuEditModel>
                {
                    Success = true,
                    Data = menuItem,
                    Message = "Menu item retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu item with ID {MenuItemId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving the menu item"
                });
            }
        }

        /// <summary>
        /// Create a new menu item
        /// </summary>
        /// <param name="request">Menu item creation request</param>
        /// <returns>Created menu item</returns>
        [HttpPost("menus")]
        [ProducesResponseType(typeof(ApiResponse<MenuModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MenuModel>>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Validation failed",
                        Message = "Invalid menu item data provided",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var menuItem = new MenuModel
                {
                    DomainID = request.DomainID,
                    Title = request.Title,
                    Icon = request.Icon,
                    PageContent = request.PageContent,
                    Action = request.Action,
                    ApiUrl = request.ApiUrl,
                    Argument = request.Argument,
                    Controller = request.Controller,
                    Description = request.Description,
                    DisplayInNavigation = request.DisplayInNavigation,
                    DisplayOrder = request.DisplayOrder,
                    ParentId = request.ParentId
                };

                // Auto-generate URL if not provided
                if (string.IsNullOrWhiteSpace(request.Url))
                {
                    menuItem.Url = request.Title.ToSlug();
                }
                else
                {
                    menuItem.Url = request.Url;
                }

                var createdMenuItem = _menuService.Save(menuItem);

                if (createdMenuItem == null)
                {
                    return StatusCode(500, new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Creation failed",
                        Message = "Failed to create the menu item"
                    });
                }

                return CreatedAtAction(
                    nameof(GetMenuItem),
                    new { id = createdMenuItem.Id },
                    new ApiResponse<MenuModel>
                    {
                        Success = true,
                        Data = createdMenuItem,
                        Message = "Menu item created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while creating the menu item"
                });
            }
        }

        /// <summary>
        /// Update an existing menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <param name="request">Menu item update request</param>
        /// <returns>Updated menu item</returns>
        [HttpPut("menus/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<MenuModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<MenuModel>>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid ID",
                        Message = "Menu item ID must be greater than 0"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Validation failed",
                        Message = "Invalid menu item data provided",
                        ValidationErrors = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                        )
                    });
                }

                var existingMenuItem = _menuService.GetAllMenuItems().FirstOrDefault(m => m.Id == id);
                if (existingMenuItem == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Menu item not found",
                        Message = $"Menu item with ID {id} was not found"
                    });
                }

                // Handle URL/Slug generation for Pages
                if (request.Controller == "Page")
                {
                    if (request.ParentId == null)
                    {
                        request.Url = request.Title.ToSlug();
                        request.Argument = request.Title.ToSlug();
                    }
                    else
                    {
                        if (request.ParentId == existingMenuItem.ParentId)
                        {
                            request.Url = $"{existingMenuItem.ParentTitle.ToSlug()}/{request.Title.ToSlug()}";
                            request.Argument = request.Url;
                        }
                    }
                }

                // Update properties
                existingMenuItem.Title = request.Title ?? existingMenuItem.Title;
                existingMenuItem.Icon = request.Icon ?? existingMenuItem.Icon;
                existingMenuItem.PageContent = request.PageContent ?? existingMenuItem.PageContent;
                existingMenuItem.Description = request.Description ?? existingMenuItem.Description;
                existingMenuItem.DisplayInNavigation = request.DisplayInNavigation;
                existingMenuItem.DisplayOrder = request.DisplayOrder;
                existingMenuItem.ParentId = request.ParentId;
                existingMenuItem.Url = request.Url;
                existingMenuItem.Controller = request.Controller ?? existingMenuItem.Controller;
                existingMenuItem.Action = request.Action ?? existingMenuItem.Action;
                existingMenuItem.Argument = request.Argument ?? existingMenuItem.Argument;

                var updatedMenuItem = _menuService.Save(existingMenuItem);

                if (updatedMenuItem == null)
                {
                    return StatusCode(500, new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Update failed",
                        Message = "Failed to update the menu item"
                    });
                }

                return Ok(new ApiResponse<MenuModel>
                {
                    Success = true,
                    Data = updatedMenuItem,
                    Message = "Menu item updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item with ID {MenuItemId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while updating the menu item"
                });
            }
        }

        /// <summary>
        /// Delete a menu item
        /// </summary>
        /// <param name="id">Menu item ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("menus/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteMenuItem(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid ID",
                        Message = "Menu item ID must be greater than 0"
                    });
                }

                var deleteResult = await _menuService.DeleteMenuAsync(id);

                if (!deleteResult)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Menu item not found or deletion failed",
                        Message = $"Menu item with ID {id} was not found or could not be deleted"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { deletedId = id },
                    Message = "Menu item deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu item with ID {MenuItemId}", id);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while deleting the menu item"
                });
            }
        }

        #endregion

        #region Convenience and Utility Endpoints

        /// <summary>
        /// Get dashboard summary statistics
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("dashboard/stats")]
        [ProducesResponseType(typeof(ApiResponse<DashboardStats>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DashboardStats>>> GetDashboardStats()
        {
            try
            {
                var websites = _websiteService.Get();
                var menuItems = _menuService.GetAllMenuItems();

                var stats = new DashboardStats
                {
                    TotalWebsites = websites.Count(),
                    TotalMenuItems = menuItems.Count(),
                    RecentWebsites = websites.OrderByDescending(w => w.ModifiedDT).Take(5).ToList(),
                    RecentMenuItems = menuItems.OrderByDescending(m => m.LastModified).Take(5).ToList(),
                    WebsitesByTemplate = websites.GroupBy(w => w.SiteTemplate ?? "Default")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    MenuItemsByDomain = menuItems.GroupBy(m => m.DomainName ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return Ok(new ApiResponse<DashboardStats>
                {
                    Success = true,
                    Data = stats,
                    Message = "Dashboard statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving dashboard statistics"
                });
            }
        }

        /// <summary>
        /// Get menu hierarchy for a specific website
        /// </summary>
        /// <param name="websiteId">Website ID</param>
        /// <returns>Hierarchical menu structure</returns>
        [HttpGet("websites/{websiteId:int}/menu-hierarchy")]
        [ProducesResponseType(typeof(ApiResponse<List<MenuHierarchyItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<MenuHierarchyItem>>>> GetMenuHierarchy(int websiteId)
        {
            try
            {
                var website = await _websiteService.GetAsync(websiteId);
                if (website == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Website not found",
                        Message = $"Website with ID {websiteId} was not found"
                    });
                }

                var menuItems = _menuService.GetAllMenuItems()
                    .Where(m => m.DomainID == websiteId)
                    .ToList();

                var hierarchy = BuildMenuHierarchy(menuItems);

                return Ok(new ApiResponse<List<MenuHierarchyItem>>
                {
                    Success = true,
                    Data = hierarchy,
                    Message = "Menu hierarchy retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving menu hierarchy for website {WebsiteId}", websiteId);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving the menu hierarchy"
                });
            }
        }

        /// <summary>
        /// Bulk update menu display orders
        /// </summary>
        /// <param name="request">Bulk update request</param>
        /// <returns>Update result</returns>
        [HttpPut("menus/bulk-update-order")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> BulkUpdateMenuOrder([FromBody] BulkUpdateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid || !request.MenuOrders.Any())
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid request",
                        Message = "Menu orders must be provided"
                    });
                }

                var updatedCount = 0;
                var errors = new List<string>();

                foreach (var orderUpdate in request.MenuOrders)
                {
                    try
                    {
                        var menuItem = _menuService.GetAllMenuItems().FirstOrDefault(m => m.Id == orderUpdate.MenuId);
                        if (menuItem != null)
                        {
                            menuItem.DisplayOrder = orderUpdate.DisplayOrder;
                            var result = _menuService.Save(menuItem);
                            if (result != null)
                            {
                                updatedCount++;
                            }
                            else
                            {
                                errors.Add($"Failed to update menu item {orderUpdate.MenuId}");
                            }
                        }
                        else
                        {
                            errors.Add($"Menu item {orderUpdate.MenuId} not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating menu item {MenuId}", orderUpdate.MenuId);
                        errors.Add($"Error updating menu item {orderUpdate.MenuId}: {ex.Message}");
                    }
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { updatedCount, errors = errors.Any() ? errors : null },
                    Message = $"Updated {updatedCount} menu items successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk update menu order");
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while updating menu orders"
                });
            }
        }

        /// <summary>
        /// Search across all content (websites and menus)
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Search results</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<SearchResults>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<SearchResults>>> Search(
            [FromQuery, Required] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        Success = false,
                        Error = "Invalid query",
                        Message = "Search query cannot be empty"
                    });
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var websites = _websiteService.Get()
                    .Where(w =>
                        w.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(w.Description) && w.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(w.Message) && w.Message.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .Select(w => new SearchResultItem
                    {
                        Id = w.Id,
                        Type = "Website",
                        Title = w.Name,
                        Description = w.Description,
                        Url = $"/WebCMS/Website/Details/{w.Id}",
                        LastModified = w.ModifiedDT
                    })
                    .ToList();

                var menuItems = _menuService.GetAllMenuItems()
                    .Where(m =>
                        m.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(m.Description) && m.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(m.PageContent) && m.PageContent.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .Select(m => new SearchResultItem
                    {
                        Id = m.Id,
                        Type = "Menu Item",
                        Title = m.Title,
                        Description = m.Description,
                        Url = $"/WebCMS/Menu/Details/{m.Id}",
                        LastModified = m.LastModified,
                        ParentInfo = m.DomainName
                    })
                    .ToList();

                var allResults = websites.Concat(menuItems)
                    .OrderByDescending(r => r.LastModified)
                    .ToList();

                var totalCount = allResults.Count;
                var pagedResults = allResults
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var searchResults = new SearchResults
                {
                    Query = query,
                    Items = pagedResults,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    WebsiteCount = websites.Count,
                    MenuItemCount = menuItems.Count
                };

                return Ok(new ApiResponse<SearchResults>
                {
                    Success = true,
                    Data = searchResults,
                    Message = $"Found {totalCount} results for '{query}'"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search with query: {Query}", query);
                return StatusCode(500, new ApiErrorResponse
                {
                    Success = false,
                    Error = "Internal server error",
                    Message = "An error occurred while performing the search"
                });
            }
        }

        #endregion

        #region Helper Methods

        private List<MenuHierarchyItem> BuildMenuHierarchy(List<MenuModel> menuItems)
        {
            var hierarchy = new List<MenuHierarchyItem>();

            // Get top-level items
            var topLevelItems = menuItems
                .Where(m => m.ParentId == null || m.ParentId == 0)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Title);

            foreach (var item in topLevelItems)
            {
                var hierarchyItem = new MenuHierarchyItem
                {
                    Id = item.Id,
                    Title = item.Title,
                    Icon = item.Icon,
                    Url = item.Url,
                    DisplayOrder = item.DisplayOrder,
                    DisplayInNavigation = item.DisplayInNavigation,
                    Children = BuildMenuChildren(menuItems, item.Id)
                };
                hierarchy.Add(hierarchyItem);
            }

            return hierarchy;
        }

        private List<MenuHierarchyItem> BuildMenuChildren(List<MenuModel> menuItems, int parentId)
        {
            var children = new List<MenuHierarchyItem>();

            var childItems = menuItems
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Title);

            foreach (var item in childItems)
            {
                var hierarchyItem = new MenuHierarchyItem
                {
                    Id = item.Id,
                    Title = item.Title,
                    Icon = item.Icon,
                    Url = item.Url,
                    DisplayOrder = item.DisplayOrder,
                    DisplayInNavigation = item.DisplayInNavigation,
                    Children = BuildMenuChildren(menuItems, item.Id)
                };
                children.Add(hierarchyItem);
            }

            return children;
        }

        #endregion
    }

    #region Request/Response Models

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Response data type</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// API error response
    /// </summary>
    public class ApiErrorResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }

    /// <summary>
    /// Paginated result wrapper
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Dashboard statistics
    /// </summary>
    public class DashboardStats
    {
        public int TotalWebsites { get; set; }
        public int TotalMenuItems { get; set; }
        public List<WebsiteModel> RecentWebsites { get; set; } = new();
        public List<MenuModel> RecentMenuItems { get; set; } = new();
        public Dictionary<string, int> WebsitesByTemplate { get; set; } = new();
        public Dictionary<string, int> MenuItemsByDomain { get; set; } = new();
    }

    /// <summary>
    /// Menu hierarchy item
    /// </summary>
    public class MenuHierarchyItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public int DisplayOrder { get; set; }
        public bool DisplayInNavigation { get; set; }
        public List<MenuHierarchyItem> Children { get; set; } = new();
    }

    /// <summary>
    /// Search results
    /// </summary>
    public class SearchResults
    {
        public string Query { get; set; } = string.Empty;
        public List<SearchResultItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int WebsiteCount { get; set; }
        public int MenuItemCount { get; set; }
    }

    /// <summary>
    /// Search result item
    /// </summary>
    public class SearchResultItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Url { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
        public string? ParentInfo { get; set; }
    }

    /// <summary>
    /// Create website request
    /// </summary>
    public class CreateWebsiteRequest
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? SiteTemplate { get; set; }

        [StringLength(100)]
        public string? SiteStyle { get; set; }

        public string? Message { get; set; }

        [StringLength(255)]
        public string? SiteName { get; set; }

        [StringLength(500)]
        [Url]
        public string? WebsiteUrl { get; set; }

        [StringLength(255)]
        public string? WebsiteTitle { get; set; }

        public bool UseBreadCrumbURL { get; set; }
        public bool IsRecipeSite { get; set; }
    }

    /// <summary>
    /// Update website request
    /// </summary>
    public class UpdateWebsiteRequest
    {
        [StringLength(255)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? SiteTemplate { get; set; }

        [StringLength(100)]
        public string? SiteStyle { get; set; }

        public string? Message { get; set; }

        [StringLength(255)]
        public string? SiteName { get; set; }

        [StringLength(500)]
        [Url]
        public string? WebsiteUrl { get; set; }

        [StringLength(255)]
        public string? WebsiteTitle { get; set; }

        public bool UseBreadCrumbURL { get; set; }
        public bool IsRecipeSite { get; set; }
    }

    /// <summary>
    /// Create menu item request
    /// </summary>
    public class CreateMenuItemRequest
    {
        [Required]
        public int DomainID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Icon { get; set; }

        public string? PageContent { get; set; }

        [StringLength(100)]
        public string? Action { get; set; }

        [StringLength(500)]
        public string? ApiUrl { get; set; }

        [StringLength(255)]
        public string? Argument { get; set; }

        [StringLength(100)]
        public string? Controller { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool DisplayInNavigation { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }

        public int? ParentId { get; set; }

        [StringLength(255)]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Update menu item request
    /// </summary>
    public class UpdateMenuItemRequest
    {
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public string? PageContent { get; set; }

        [StringLength(100)]
        public string? Action { get; set; }

        [StringLength(500)]
        public string? ApiUrl { get; set; }

        [StringLength(255)]
        public string? Argument { get; set; }

        [StringLength(100)]
        public string? Controller { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool DisplayInNavigation { get; set; }

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }

        public int? ParentId { get; set; }

        [StringLength(255)]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Bulk update order request
    /// </summary>
    public class BulkUpdateOrderRequest
    {
        [Required]
        public List<MenuOrderUpdate> MenuOrders { get; set; } = new();
    }

    /// <summary>
    /// Menu order update item
    /// </summary>
    public class MenuOrderUpdate
    {
        [Required]
        public int MenuId { get; set; }

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }
    }

    #endregion
}