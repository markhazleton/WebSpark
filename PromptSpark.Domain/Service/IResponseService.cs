using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service;

public interface IResponseService
{
    Task<List<DefinitionResponseDto>> GetAllResponsesAsync();
    Task<DefinitionResponseDto> GetResponseByIdAsync(int? id);
    Task AddResponseAsync(DefinitionResponseDto gPTResponse);
    Task UpdateResponseAsync(DefinitionResponseDto gPTResponse);
    Task DeleteResponseAsync(int id);
    bool ResponseExists(int id);
}
