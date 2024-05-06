using WebSpark.Prompt.Models;

namespace WebSpark.Prompt.Service
{
    public interface IGPTDefinitionTypeService
    {
        Task DeleteGPTDefinitionType(string definitionType);
        Task<List<DefinitionTypeDto>> GetAllGPTDefinitionTypes();
        Task<DefinitionTypeDto?> GetGPTDefinitionTypeByKey(string definitionType);
        Task UpdateGPTDefinitionType(DefinitionTypeDto definitionType);
    }
}
