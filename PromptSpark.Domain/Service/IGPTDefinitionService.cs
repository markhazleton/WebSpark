using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service;

public interface IGPTDefinitionService
{
    Task<List<DefinitionDto>> GetDefinitionsAsync();
    Task<DefinitionDto> GetDefinitionDtoAsync(int id);
    Task<DefinitionDto> CreateAsync(DefinitionDto definitionDto);
    Task<DefinitionDto> UpdateDefinitionAsync(DefinitionDto definitionDto);
    Task<bool> DeleteDefinitionAsync(int definitionId);
    Task<bool> GPTDefinitionExists(int id);
    Task<DefinitionDto> RefreshDefinitionResponses(int id);
    Task<int> CreateDefinitionHash();
}
