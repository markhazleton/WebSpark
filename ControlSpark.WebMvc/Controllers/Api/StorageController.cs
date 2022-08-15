using Microsoft.AspNetCore.Authorization;

namespace ControlSpark.WebMvc.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class StorageController : ControllerBase
{
    private readonly IAuthorProvider _authorProvider;
    private readonly IBlogProvider _blogProvider;
    private readonly IPostProvider _postProvider;
    private readonly IStorageProvider _storageProvider;

    public StorageController(IStorageProvider storageProvider, IAuthorProvider authorProvider, IBlogProvider blogProvider, IPostProvider postProvider)
    {
        _storageProvider = storageProvider;
        _authorProvider = authorProvider;
        _blogProvider = blogProvider;
        _postProvider = postProvider;
    }

    [Authorize]
    [HttpPut("exists")]
    public async Task<IActionResult> FileExists([FromBody] string path)
    {
        return await Task.FromResult(_storageProvider.FileExists(path)) ? Ok() : BadRequest();
    }

    [Authorize]
    [HttpGet("themes")]
    public async Task<IList<string>> GetThemes()
    {
        return await _storageProvider.GetThemes();
    }

    [Authorize]
    [HttpPost("upload/{uploadType}")]
    public async Task<ActionResult> Upload(IFormFile file, UploadType uploadType, int postId = 0)
    {
        if (file == null)
            return BadRequest();

        var author = await _authorProvider.FindByEmail(User.Identity.Name);
        var post = postId == 0 ? new Post() : await _postProvider.GetPostById(postId);

        var path = $"{author.Id}/{DateTime.Now.Year}/{DateTime.Now.Month}";
        var fileName = $"data/{path}/{file.FileName}";

        if (uploadType == UploadType.PostImage)
            fileName = $"{Url.Content("~/")}{fileName}";

        if (await _storageProvider.UploadFormFile(file, path))
        {
            var blog = await _blogProvider.GetBlog();

            switch (uploadType)
            {
                case UploadType.Avatar:
                    author.Avatar = fileName;
                    return await _authorProvider.Update(author) ? new JsonResult(fileName) : BadRequest();
                case UploadType.AppLogo:
                    blog.Logo = fileName;
                    return await _blogProvider.Update(blog) ? new JsonResult(fileName) : BadRequest();
                case UploadType.AppCover:
                    blog.Cover = fileName;
                    return await _blogProvider.Update(blog) ? new JsonResult(fileName) : BadRequest();
                case UploadType.PostCover:
                    post.Cover = fileName;
                    return new JsonResult(fileName);
                case UploadType.PostImage:
                    return new JsonResult(fileName);
            }
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
}
