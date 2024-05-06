using Microsoft.EntityFrameworkCore;
using WebSpark.Prompt.Data;
using WebSpark.Prompt.Models;

namespace WebSpark.Prompt.Service;

public class UserPromptService(GPTDbContext context, IGPTService gPTService)
: IUserPromptService, IDisposable
{
    private bool disposedValue;

    public async Task<UserPromptDto> RefreshDefinitionResponses(int id)
    {
        var userPromptDto = await context.Chats
            .Include(c => c.GPTResponses)
            .Where(c => c.Id == id)
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
                DefinitionResponses = new List<DefinitionResponseDto>()
            };
        }
        var entity = await context.Chats
            .Where(c => c.Id == id)
            .Select(c => new
            {
                Chat = c,
                GPTResponses = c.GPTResponses.Where(r => r.DefinitionType == c.DefinitionType).ToList()
            })
            .FirstOrDefaultAsync();

        if (entity != null)
        {
            entity.Chat.GPTResponses = entity.GPTResponses;
        }

        return entity.Chat?.ToDto(definitionTypes);
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

    public async Task CreateOrUpdateAsync(UserPromptDto dto)
    {
        if (dto.Id == 0)
        {
            await CreateAsync(dto);
            return;
        }

        var entity = await context.Chats.FindAsync(dto.Id);
        if (entity != null)
        {
            entity.UserPrompt = dto.UserPrompt;
            entity.DefinitionType = dto.DefinitionType;
            entity.Updated = DateTime.Now;  // Or dto.Updated
            await context.SaveChangesAsync();
        }
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (IsDbContextAvailable(context))
                {
                    context.ChangeTracker.Clear();
                    context.Database.CloseConnection();
                    context.Dispose();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }
    bool IsDbContextAvailable(DbContext context)
    {
        try
        {
            // Attempt to access the database connection
            var connectionState = context.Database.GetDbConnection().State;
            // If no exception is thrown, the context is not disposed
            return true;
        }
        catch (ObjectDisposedException)
        {
            // An ObjectDisposedException was thrown, indicating the context is disposed
            return false;
        }
        catch
        {
            // Other exceptions might indicate different issues (e.g., connection problems)
            return false; // Or handle differently
        }
    }


    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~UserPromptService()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
