using PromptSpark.Domain.Data;
using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service;
public interface IGPTService
{
    Task<UserPromptDto> FindResponseByUserPromptTextAsync(string userPrompt);
    Task<UserPromptDto> FindResponseByUserPromptIdAsync(int id);
    Task<List<UserPromptDto>> GetUserPromptsByDefinitionTypeAsync(string definitionType);
    Task<UserPromptDto> RefreshGPTResponse(UserPromptDto req);
    Task<UserPromptDto> RefreshUserPromptResponses(UserPromptDto req);
    Task RerunAllPrompts();
    Task<GPTDefinitionResponse> UpdateGPTResponse(GPTDefinitionResponse gptResponse, CancellationToken ct = default);
    Task<GPTDefinitionResponse> UpdateGPTResponseJson<T>(GPTDefinitionResponse gptResponse, CancellationToken ct = default);
}

