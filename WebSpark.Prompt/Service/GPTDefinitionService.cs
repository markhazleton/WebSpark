using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebSpark.Prompt.Data;
using WebSpark.Prompt.Models;

namespace WebSpark.Prompt.Service;

public class GPTDefinitionService(GPTDbContext context, IGPTService service) : IGPTDefinitionService, IDisposable
{
    private bool disposedValue;

    public async Task<DefinitionDto> CreateAsync(DefinitionDto definitionDto)
    {
        var gptDefinition = new GPTDefinition
        {
            GPTName = definitionDto.Name,
            Description = definitionDto.Description,
            Prompt = definitionDto.Prompt,
            PromptHash = definitionDto.Prompt.GetHashCode().ToString(),
            DefinitionType = definitionDto.DefinitionType,
            Role = definitionDto.Role,
            Model = definitionDto.Model,
            Temperature = definitionDto.Temperature
        };

        context.Definitions.Add(gptDefinition);
        await context.SaveChangesAsync();

        definitionDto.DefinitionId = gptDefinition.DefinitionId; // Return the ID of the newly created entity
        return definitionDto;
    }

    public async Task<DefinitionDto> UpdateDefinitionAsync(DefinitionDto definitionDto)
    {
        var existingDefinition = await context.Definitions.FindAsync(definitionDto.DefinitionId);
        if (existingDefinition == null)
        {
            existingDefinition = new GPTDefinition
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                GPTName = definitionDto.Name,
                Description = definitionDto.Description,
                Prompt = definitionDto.Prompt,
                DefinitionType = definitionDto.DefinitionType,
                OutputType = definitionDto.OutputType,
                Role = definitionDto.Role,
                Model = definitionDto.Model,
                Temperature = definitionDto.Temperature
            };
            context.Definitions.Add(existingDefinition);
        }
        else
        {
            existingDefinition.GPTName = definitionDto.Name;
            existingDefinition.Description = definitionDto.Description;
            existingDefinition.Prompt = definitionDto.Prompt;
            existingDefinition.DefinitionType = definitionDto.DefinitionType;
            existingDefinition.OutputType = definitionDto.OutputType;
            existingDefinition.Role = definitionDto.Role;
            existingDefinition.Model = definitionDto.Model;
            existingDefinition.Temperature = definitionDto.Temperature;
            existingDefinition.Updated = DateTime.UtcNow;
            context.Definitions.Update(existingDefinition);
        }
        await context.SaveChangesAsync();
        return definitionDto;
    }
    public async Task<bool> GPTDefinitionExists(int id)
    {
        return await context.Definitions.AnyAsync(e => e.DefinitionId == id);
    }
    public async Task<List<DefinitionDto>> GetDefinitionsAsync()
    {
        List<GPTDefinition> listDb = [];
        List<DefinitionDto> listDto = [];
        listDb = await context.Definitions.ToListAsync() ?? [];
        try
        {
            foreach (var definition in listDb)
            {
                var definitionDto = definition.ToDto();
                listDto.Add(definitionDto);
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
        }
        return listDto ?? [];
    }
    public async Task<int> CreateDefinitionHash()
    {
        int newHash = 0;
        var definitions = await context.Definitions.ToListAsync();
        foreach (var def in definitions)
        {
            var hash = def.Prompt.GetHashCode().ToString();
            if (def.PromptHash != hash)
            {
                newHash++;
                def.PromptHash = hash;
                context.Definitions.Update(def);
                await context.SaveChangesAsync();
            }
        }
        return newHash;
    }
    public static string GetMd5HashBase64(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(hashBytes);
        }
    }
    public async Task<DefinitionDto> RefreshDefinitionResponses(int id)
    {
        try
        {
            var definition = await context.Definitions
                .Include(d => d.GPTResponses)
                .ThenInclude(r => r.Response)
                .Where(d => d.DefinitionId == id)
                .Select(s => s.ToDto())
                .SingleOrDefaultAsync();

            if (definition == null) return new();

            List<UserPromptDto> userPrompts = await context
                .Chats
                .Where(w => w.DefinitionType == definition.DefinitionType)
                .Select(s => s.ToDto())
                .ToListAsync();

            List<Task> tasks = [];
            foreach (var userPrompt in userPrompts)
            {
                tasks.Add(ProcessDefinitionUserPrompt(definition, userPrompt));
            }
            await Task.WhenAll(tasks);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new();
        }
        return await context.Definitions
            .Include(d => d.GPTResponses)
            .ThenInclude(r => r.Response)
            .Where(d => d.DefinitionId == id)
            .Select(s => s.ToDto())
            .SingleOrDefaultAsync() ?? new DefinitionDto();
    }

    private async Task ProcessDefinitionUserPrompt(DefinitionDto definition, UserPromptDto userPrompt)
    {
        // check for existing response
        var existingResponse = definition.DefinitionResponses
            .Where(r => r.UserPrompt == userPrompt.UserPrompt)
            .SingleOrDefault();

        if (existingResponse == null)
        {
            var dbDefinition = context.Definitions.Where(w => w.DefinitionId == definition.DefinitionId).FirstOrDefault();
            var dbUserPrompt = context.Chats.Where(w => w.UserPrompt == userPrompt.UserPrompt).FirstOrDefault();
            GPTDefinitionResponse definitionResponse = new(dbDefinition, dbUserPrompt);
            definitionResponse = await service.UpdateGPTResponse(definitionResponse);
            context.DefinitionResponses.Add(definitionResponse);
            await context.SaveChangesAsync();
        }
        else
        {
            GPTDefinitionResponse? existingDbResponse = context.DefinitionResponses
                .Where(w => w.DefinitionId == definition.DefinitionId
                && w.UserPrompt == userPrompt.UserPrompt)
                .FirstOrDefault();
            if (existingDbResponse != null)
            {
                existingDbResponse.SystemPrompt = definition.Prompt;
                existingDbResponse.UserPrompt = userPrompt.UserPrompt;
                existingDbResponse.UserExpectedResponse = userPrompt.UserExpectedResponse;
                existingDbResponse.Updated = DateTime.UtcNow;
                existingDbResponse.Temperature = definition.Temperature;
                existingDbResponse.OutputType = definition.OutputType;
                existingDbResponse.DefinitionType = definition.DefinitionType;
                existingDbResponse.GPTName = definition.Name;
                existingDbResponse.Model = definition.Model;
                existingDbResponse.Temperature = definition.Temperature;
                existingDbResponse = await service.UpdateGPTResponse(existingDbResponse);
                context.DefinitionResponses.Update(existingDbResponse);
                await context.SaveChangesAsync();
            }
        }
    }

    public async Task<DefinitionDto> GetDefinitionDtoAsync(int id)
    {
        DefinitionDto resultDefinition = new();
        if (id > 0)
        {
            resultDefinition = await context.Definitions
                .Include(d => d.GPTResponses)
                .ThenInclude(r => r.Response)
                .Where(d => d.DefinitionId == id)
                .Select(d => d.ToDto()).SingleOrDefaultAsync() ?? new();
        }
        resultDefinition.DefinitionTypes = await context.DefinitionTypes
            .Select(d => d.DefinitionType).Distinct()
            .ToListAsync() ?? ["Wichita"];

        return resultDefinition;
    }
    public async Task<bool> DeleteDefinitionAsync(int definitionId)
    {
        var definition = await context.Definitions.FindAsync(definitionId);
        if (definition == null) return false;

        context.Definitions.Remove(definition);
        await context.SaveChangesAsync();
        return true;
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
    ~GPTDefinitionService()
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
