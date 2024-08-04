using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service;

public interface IGPTDefinitionTypeService
{
    Task DeleteGPTDefinitionType(string definitionType);
    Task<DefinitionResponseDto> FindDefinitionResponseByIdAsync(int id);
    Task<UserPromptDto> FindUserPromptByUserPromptIdAsync(int id);
    Task<List<DefinitionTypeDto>> GetAllGPTDefinitionTypes();
    Task<DefinitionTypeDto?> GetGPTDefinitionTypeByKey(string definitionType);
    Task UpdateGPTDefinitionType(DefinitionTypeDto definitionType);

}