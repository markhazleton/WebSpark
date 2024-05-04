using PromptSpark.Areas.OpenAI.Models;

namespace PromptSpark.Areas.OpenAI.Service
{
    public interface IGPTDefinitionTypeService
    {
        Task DeleteGPTDefinitionType(string definitionType);
        Task<List<DefinitionTypeDto>> GetAllGPTDefinitionTypes();
        Task<DefinitionTypeDto?> GetGPTDefinitionTypeByKey(string definitionType);
        Task UpdateGPTDefinitionType(DefinitionTypeDto definitionType);
    }
}
