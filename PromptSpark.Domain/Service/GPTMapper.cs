using PromptSpark.Domain.Data;
using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service;

public static class GPTMapper
{
    public static DefinitionTypeDto ToDto(this GPTDefinitionType definitionType)
    {
        if (definitionType == null) return new DefinitionTypeDto();
        return new DefinitionTypeDto
        {
            DefinitionType = definitionType.DefinitionType,
            Description = definitionType.Description,
            OutputType = definitionType.OutputType,
            Created = definitionType.Created,
            Updated = definitionType.Updated,
        };
    }
    public static GPTDefinitionType ToEntity(this DefinitionTypeDto definitionType)
    {
        if (definitionType == null) return new GPTDefinitionType();
        return new GPTDefinitionType
        {
            DefinitionType = definitionType.DefinitionType,
            OutputType = definitionType.OutputType,
            Description = definitionType.Description ?? definitionType.DefinitionType,
            Created = definitionType.Created,
            Updated = definitionType.Updated,
        };
    }


    public static DefinitionDto ToDto(this GPTDefinition definition)
    {
        if (definition == null) return new DefinitionDto();

        definition.GPTResponses ??= [];

        return new DefinitionDto
        {
            DefinitionId = definition.DefinitionId,
            DefinitionType = definition.DefinitionType,
            Name = definition.GPTName,
            OutputType = definition.OutputType,
            Description = definition.Description,
            Role = definition.Role,
            Temperature = definition.Temperature,
            Model = definition.Model,
            Prompt = definition.Prompt,
            PromptHash = definition.Prompt.GetHashCode().ToString(),
            Created = definition.Created,
            Updated = definition.Updated,
            DefinitionResponses = definition.GPTResponses.Select(r => ToDto(r)).ToList()
        };
    }
    public static UserPromptDto ToDto(this GPTUserPrompt prompt, List<string> definitionTypes)
    {
        if (prompt == null) return new UserPromptDto();

        var dto = new UserPromptDto
        {
            UserPromptId = prompt.Id,
            UserPrompt = prompt.UserPrompt,
            UserExpectedResponse = prompt.UserExpectedResponse,
            DefinitionType = prompt.DefinitionType,
            Created = prompt.Created,
            Updated = prompt.Updated,
            DefinitionResponses = prompt.GPTResponses
                .Select(r => r.ToDto()) // Make sure you have implemented ToDto for GPTDefinitionResponse to DefinitionResponseDto conversion
                .ToList(),
            // Populate the DefinitionTypes field either with the passed list or by defaulting to a list containing only the current DefinitionType
            DefinitionTypes = definitionTypes ?? [prompt.DefinitionType]
        };

        return dto;
    }

    public static UserPromptDto ToDto(this GPTUserPrompt response)
    {
        if (response == null) return new UserPromptDto();
        return new UserPromptDto
        {
            UserPromptId = response.Id,
            UserPrompt = response.UserPrompt,
            DefinitionType = response.DefinitionType,
            UserExpectedResponse = response.UserExpectedResponse,
            Created = response.Created,
            Updated = response.Updated,
            DefinitionResponses = response.GPTResponses
            .Select(r => r.ToDto()).ToList()
        };
    }
    public static GPTUserPrompt ToEntity(this UserPromptDto dto)
    {
        return new GPTUserPrompt
        {
            Id = dto.UserPromptId,
            UserPrompt = dto.UserPrompt,
            UserExpectedResponse = dto.UserExpectedResponse,
            DefinitionType = dto.DefinitionType,
            Created = dto.Created,
            Updated = dto.Updated
        };
    }
    public static DefinitionResponseDto ToDto(this GPTDefinitionResponse r)
    {
        return new DefinitionResponseDto
        {
            GPTName = r.GPTName,
            Model = r.Model,
            SystemResponse = r.SystemResponse,
            SystemPrompt = r.SystemPrompt ?? "MISSING",
            UserPromptId = r.ResponseId,
            UserPrompt = r.UserPrompt ?? "MISING",
            Created = r.Created,
            Updated = r.Updated,
            TimeMS = r.ElapsedMilliseconds,
            TotalTokens = r.TotalTokens,
            PromptTokens = r.PromptTokens,
            CompletionTokens = r.CompletionTokens,
            Temperature = r.Temperature,
            MessageType = r.DefinitionType,
            OutputType = r.OutputType,
            GPTDescription = r.GPTDescription,
            UserExpectedResponse = r.UserExpectedResponse
        };
    }
}
