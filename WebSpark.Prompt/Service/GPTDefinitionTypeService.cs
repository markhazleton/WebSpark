using Microsoft.EntityFrameworkCore;
using WebSpark.Prompt.Data;
using WebSpark.Prompt.Models;

namespace WebSpark.Prompt.Service;
public class GPTDefinitionTypeService(GPTDbContext context) : IGPTDefinitionTypeService, IDisposable
{
    private bool disposedValue;

    // Delete a GPTDefinitionType record
    public async Task DeleteGPTDefinitionType(string definitionType)
    {
        if (string.IsNullOrWhiteSpace(definitionType)) throw new ArgumentException("Invalid definition type", nameof(definitionType));

        var typeToDelete = await context.DefinitionTypes.FirstOrDefaultAsync(dt => dt.DefinitionType == definitionType) ?? throw new InvalidOperationException("DefinitionType not found");
        context.DefinitionTypes.Remove(typeToDelete);
        await context.SaveChangesAsync();
    }

    // Read all GPTDefinitionType records
    public async Task<List<DefinitionTypeDto>> GetAllGPTDefinitionTypes()
    {
        return await context.DefinitionTypes.Select(s => s.ToDto()).ToListAsync()
            ?? [];
    }

    // Read a single GPTDefinitionType record by DefinitionType
    public async Task<DefinitionTypeDto?> GetGPTDefinitionTypeByKey(string definitionType)
    {
        if (string.IsNullOrWhiteSpace(definitionType))
            throw new ArgumentException("Invalid definition type", nameof(definitionType));

        var definitionTypeReturn = await context.DefinitionTypes
            .Where(w => w.DefinitionType == definitionType)
            .Select(s => s.ToDto())
            .FirstOrDefaultAsync() ?? new DefinitionTypeDto();

        definitionTypeReturn.Definitions = (await context.Definitions
            .Where(w => w.DefinitionType == definitionType)
            .Select(s => s.ToDto())
            .ToListAsync()) ?? [];

        definitionTypeReturn.Prompts = (await context.Chats.Where(w => w.DefinitionType == definitionType)
            .Select(s => s.ToDto())
            .ToListAsync()) ?? [];

        return definitionTypeReturn;

    }

    // Update an existing GPTDefinitionType record
    public async Task UpdateGPTDefinitionType(DefinitionTypeDto definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);
        if (string.IsNullOrWhiteSpace(definitionType.DefinitionType)) throw new ArgumentException("Invalid definition type", nameof(definitionType));

        var existingType = await context.DefinitionTypes
            .FirstOrDefaultAsync(dt => dt.DefinitionType == definitionType.DefinitionType);

        if (existingType == null)
        {
            ArgumentNullException.ThrowIfNull(definitionType);
            context.DefinitionTypes.Add(definitionType.ToEntity());
            await context.SaveChangesAsync();
        }
        else
        {
            existingType.Description = definitionType?.Description ?? "MISSING";
            existingType.OutputType = definitionType?.OutputType ?? OutputType.Markdown;
            existingType.Updated = DateTime.Now;
            context.DefinitionTypes.Update(existingType);
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
    ~GPTDefinitionTypeService()
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

