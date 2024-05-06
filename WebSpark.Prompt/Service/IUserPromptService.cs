using WebSpark.Prompt.Models;

namespace WebSpark.Prompt.Service
{
    public interface IUserPromptService
    {
        Task<UserPromptDto> CreateAsync(UserPromptDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<UserPromptDto>> GetAllAsync();
        Task<UserPromptDto> ReadAsync(int id);
        Task<UserPromptDto> RefreshDefinitionResponses(int id);
        Task CreateOrUpdateAsync(UserPromptDto dto);
    }
}
