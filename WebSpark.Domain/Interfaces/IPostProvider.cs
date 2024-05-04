using WebSpark.Domain.Entities;
using WebSpark.Domain.Models;

namespace WebSpark.Domain.Interfaces;

public interface IPostProvider
{
    Task<List<Post>> GetPosts(PublishedStatus filter, PostType postType);
    Task<List<Post>> SearchPosts(string term);
    Task<Post> GetPostById(int id);
    Task<Post> GetPostBySlug(string slug);
    Task<string> GetSlugFromTitle(string title);
    Task<bool> Add(Post post);
    Task<bool> Update(Post post);
    Task<bool> Publish(int id, bool publish);
    Task<bool> Featured(int id, bool featured);
    Task<IEnumerable<PostItem>> GetPostItems();
    Task<PostModel> GetPostModel(string slug);
    Task<List<PostItem>> GetAllPostsAsync();
    Task<IEnumerable<PostItem>> GetPopular(Pager pager, int author = 0);
    Task<IEnumerable<PostItem>> Search(Pager pager, string term, int author = 0, string include = "", bool sanitize = false);
    Task<IEnumerable<PostItem>> GetList(Pager pager, int author = 0, string category = "", string include = "", bool sanitize = true);
    Task<bool> Remove(int id);
}
