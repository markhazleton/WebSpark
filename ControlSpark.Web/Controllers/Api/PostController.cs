using Microsoft.AspNetCore.Authorization;

namespace ControlSpark.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    private readonly IPostProvider _postProvider;

    public PostController(IPostProvider postProvider)
    {
        _postProvider = postProvider;
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<ActionResult<bool>> AddPost(Post post)
    {
        return await _postProvider.Add(post);
    }

    [Authorize]
    [HttpPut("featured/{id:int}")]
    public async Task<ActionResult<bool>> FeaturedPost(int id, [FromBody] bool featured)
    {
        return await _postProvider.Featured(id, featured);
    }

    [HttpGet("byslug/{slug}")]
    public async Task<ActionResult<Post>> GetPostBySlug(string slug)
    {
        return await _postProvider.GetPostBySlug(slug);
    }

    [HttpGet("list/{filter}/{postType}")]
    public async Task<ActionResult<List<Post>>> GetPosts(PublishedStatus filter, PostType postType)
    {
        return await _postProvider.GetPosts(filter, postType);
    }

    [HttpGet("getslug/{title}")]
    public async Task<ActionResult<string>> GetSlug(string title)
    {
        return await _postProvider.GetSlugFromTitle(title);
    }

    [Authorize]
    [HttpPut("publish/{id:int}")]
    public async Task<ActionResult<bool>> PublishPost(int id, [FromBody] bool publish)
    {
        return await _postProvider.Publish(id, publish);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<bool>> RemovePost(int id)
    {
        return await _postProvider.Remove(id);
    }

    [HttpGet("list/search/{term}")]
    public async Task<ActionResult<List<Post>>> SearchPosts(string term)
    {
        return await _postProvider.SearchPosts(term);
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<ActionResult<bool>> UpdatePost(Post post)
    {
        return await _postProvider.Update(post);
    }
}
