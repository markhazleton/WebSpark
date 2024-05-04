using Microsoft.AspNetCore.Authorization;
using WebSpark.Domain.Entities;
using WebSpark.Domain.Interfaces;

namespace WebSpark.WebMvc.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class BlogController(IBlogProvider blogProvider) : ControllerBase
{
    private readonly IBlogProvider _blogProvider = blogProvider;

    [HttpGet]
    public async Task<Blog> GetBlog()
    {
        return await _blogProvider.GetBlog();
    }

    [HttpGet("categories")]
    public async Task<ICollection<Category>> GetBlogCategories()
    {
        return await _blogProvider.GetBlogCategories();
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<bool>> ChangeTheme([FromBody] Blog blog)
    {
        return await _blogProvider.Update(blog);
    }
}
