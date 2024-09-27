using Microsoft.EntityFrameworkCore;
using PromptSpark.Domain.Data;
using PromptSpark.Domain.Models;

namespace PromptSpark.Domain.Service;

public class UserPromptService(GPTDbContext context, IGPTService gPTService)
    : IUserPromptService
{
    public async Task<UserPromptDto> RefreshDefinitionResponses(int UserPromptId)
    {
        var userPromptDto = await context.Chats
            .Include(c => c.GPTResponses)
            .Where(c => c.Id == UserPromptId)
            .Select(c => c.ToDto())
            .FirstOrDefaultAsync();

        if (userPromptDto != null)
        {
            userPromptDto = await gPTService.RefreshUserPromptResponses(userPromptDto);
            return userPromptDto;
        }
        return new UserPromptDto();
    }

    public async Task<UserPromptDto> CreateAsync(UserPromptDto dto)
    {
        dto.Created = DateTime.Now;
        dto.Updated = DateTime.Now;
        var entity = dto.ToEntity();

        // Check for duplicate user prompt and definition type
        var existing = await context.Chats
            .Where(c => c.UserPrompt == entity.UserPrompt && c.DefinitionType == entity.DefinitionType)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            return existing.ToDto();
        }

        context.Chats.Add(entity);
        await context.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<UserPromptDto> ReadAsync(int id)
    {
        var definitionTypes = await GetDefinitionTypes();

        if (id == 0)
        {
            return new UserPromptDto
            {
                DefinitionType = definitionTypes.FirstOrDefault(),
                DefinitionTypes = definitionTypes,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                DefinitionResponses = []
            };
        }

        var entity = await context.Chats
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (entity != null)
        {
            var responses = await context.DefinitionResponses
                .Where(w => w.UserPrompt == entity.UserPrompt && w.DefinitionType == entity.DefinitionType)
                .ToListAsync();

            entity.GPTResponses = responses;
        }

        return entity.ToDto(definitionTypes);
    }

    private async Task<List<string>> GetDefinitionTypes()
    {
        return await context.DefinitionTypes.Select(dt => dt.DefinitionType).ToListAsync();
    }

    public async Task<IEnumerable<UserPromptDto>> GetAllAsync()
    {
        var entities = await context.Chats.Include(c => c.GPTResponses).ToListAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<UserPromptDto> CreateOrUpdateAsync(UserPromptDto dto)
    {
        if (dto.UserPromptId == 0)
        {
            return await CreateAsync(dto);
        }

        var entity = await context.Chats.FindAsync(dto.UserPromptId);
        if (entity != null)
        {
            entity.UserPrompt = dto.UserPrompt;
            entity.DefinitionType = dto.DefinitionType;
            entity.Updated = DateTime.Now;
            await context.SaveChangesAsync();
        }
        return entity?.ToDto() ?? dto;

    }

    public async Task DeleteAsync(int id)
    {
        var entity = await context.Chats.FindAsync(id);
        if (entity != null)
        {
            context.Chats.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
