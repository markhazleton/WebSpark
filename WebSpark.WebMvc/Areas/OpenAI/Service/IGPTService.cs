using PromptSpark.Areas.OpenAI.Data;
using PromptSpark.Areas.OpenAI.Models;

namespace PromptSpark.Areas.OpenAI.Service;
public interface IGPTService
{
    Task<UserPromptDto> FindResponseByUserPromptTextAsync(string userPrompt);
    Task<List<UserPromptDto>> GetUserPromptsByDefinitionTypeAsync(string definitionType);
    Task<UserPromptDto> RefreshGPTResponse(UserPromptDto req);
    Task<UserPromptDto> RefreshUserPromptResponses(UserPromptDto req);
    Task RerunAllPrompts();
    Task<GPTDefinitionResponse> UpdateGPTResponse(GPTDefinitionResponse gptResponse);
}
