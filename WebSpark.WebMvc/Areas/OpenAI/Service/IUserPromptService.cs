using PromptSpark.Areas.OpenAI.Models;

namespace PromptSpark.Areas.OpenAI.Service
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
