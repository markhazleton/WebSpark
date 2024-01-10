using Microsoft.AspNetCore.Authorization;

namespace ControlSpark.WebMvc.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryProvider categoryProvider) : ControllerBase
{
    private readonly ICategoryProvider _categoryProvider = categoryProvider;

    [Authorize]
    [HttpPost("{postId:int}/{tag}")]
    public async Task<ActionResult<bool>> AddPostCategory(int postId, string tag)
    {
        return await _categoryProvider.AddPostCategory(postId, tag);
    }

    [HttpGet]
    public async Task<List<CategoryItem>> GetCategories()
    {
        return await _categoryProvider.Categories();
    }

    [HttpGet("byId/{categoryId:int}")]
    public async Task<Category> GetCategory(int categoryId)
    {
        return await _categoryProvider.GetCategory(categoryId);
    }

    [HttpGet("{postId:int}")]
    public async Task<ICollection<Category>> GetPostCategories(int postId)
    {
        return await _categoryProvider.GetPostCategories(postId);
    }

    [Authorize]
    [HttpDelete("{categoryId:int}")]
    public async Task<ActionResult<bool>> RemoveCategory(int categoryId)
    {
        return await _categoryProvider.RemoveCategory(categoryId);
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<bool>> SaveCategory(Category category)
    {
        return await _categoryProvider.SaveCategory(category);
    }

    [Authorize]
    [HttpPut("{postId:int}")]
    public async Task<ActionResult<bool>> SavePostCategories(int postId, List<Category> categories)
    {
        return await _categoryProvider.SavePostCategories(postId, categories);
    }

    [HttpGet("{term}")]
    public async Task<List<CategoryItem>> SearchCategories(string term = "*")
    {
        return await _categoryProvider.SearchCategories(term);
    }
}
