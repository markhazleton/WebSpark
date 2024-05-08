using HttpClientUtility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebSpark.Prompt.Data;
using WebSpark.Prompt.Models;
using WebSpark.Prompt.Models.OpenAI;

namespace WebSpark.Prompt.Service;

/// <summary>
/// Represents the service for GPT.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OpenAIChatCompletionService"/> class.
/// </remarks>
/// <param name="httpClientService">The HTTP client service.</param>
/// <param name="configuration">The configuration.</param>
/// <param name="context">The GPT database context.</param>
public class OpenAIChatCompletionService(
    IHttpClientService httpClientService,
    IConfiguration configuration,
    GPTDbContext context) : IGPTService, IDisposable
{
    private readonly Dictionary<string, string> headers = new() { { "Authorization", $"Bearer {configuration.GetValue<string>("OPENAI_API_KEY") ?? "not found"}" } };
    private readonly Uri openAiUrl = new(configuration.GetValue<string>("OPENAI_URL") ?? "https://api.openai.com/v1/chat/completions");
    private bool disposedValue;

    private static OpenAiApiRequest GetOpenAiApiRequest(GPTDefinitionResponse definitionResponse)
    {
        if (Double.TryParse(definitionResponse.Temperature, out double temperature) == false)
            throw new Exception("temperature is not a valid double.");

        var systemMessage = new Message()
        {
            role = "system",
            content = definitionResponse.SystemPrompt,
        };
        if (definitionResponse.OutputType.ToString().ToLower() == "json")
        {
            return new OpenAiApiRequest()
            {
                model = definitionResponse.Model,
                response_format = new ResponseFormat() { type = "json_object" },
                messages =
                    [
                        systemMessage,
                    new Message
                    {
                        role = "user",
                        content = definitionResponse.UserPrompt
                    }
                    ],
                temperature = temperature
            };
        }
        else
        {
            return new OpenAiApiRequest()
            {
                model = definitionResponse.Model,
                response_format = new ResponseFormat() { type = "text" },
                messages =
                    [
                        systemMessage,
                    new Message
                    {
                        role = "user",
                        content = definitionResponse.UserPrompt
                    }
                    ],
                temperature = temperature
            };
        }
    }
    private static OpenAiApiRequest GetOpenAiApiRequest(DefinitionResponseDto definitionResponse)
    {
        if (Double.TryParse(definitionResponse.Temperature, out double temperature) == false)
            throw new Exception("temperature is not a valid double.");

        var systemMessage = new Message()
        {
            role = "system",
            content = definitionResponse.SystemPrompt,
        };
        if (definitionResponse.OutputType.ToString().ToLower() == "json")
        {
            return new OpenAiApiRequest()
            {
                model = definitionResponse.Model,
                response_format = new ResponseFormat() { type = "json_object" },
                messages =
                    [
                        systemMessage,
                    new Message
                    {
                        role = "user",
                        content = definitionResponse.UserPrompt
                    }
                    ],
                temperature = temperature
            };
        }
        else
        {
            return new OpenAiApiRequest()
            {
                model = definitionResponse.Model,
                response_format = new ResponseFormat() { type = "text" },
                messages =
                    [
                        systemMessage,
                    new Message
                    {
                        role = "user",
                        content = definitionResponse.UserPrompt
                    }
                    ],
                temperature = temperature
            };
        }
    }

    public async Task<UserPromptDto> FindResponseByUserPromptTextAsync(string userPrompt)
    {
        try
        {
            var userPromptId = await context.Chats
                .Where(w => w.UserPrompt == userPrompt)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (userPromptId == 0)
                return new UserPromptDto();

            return await FindResponseByUserPromptIdAsync(userPromptId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new UserPromptDto();

        }
    }
    public async Task<UserPromptDto> FindResponseByUserPromptIdAsync(int id)
    {
        // Attempt to retrieve the userPrompt from the database
        var response = await context.Chats
            .FirstOrDefaultAsync(r => r.Id == id);

        if (response == null)
            return new UserPromptDto();

        response.GPTResponses = await context.DefinitionResponses
            .Where(w => w.UserPrompt == response.UserPrompt &&
            w.DefinitionType == response.DefinitionType)
            .Include(i => i.Definition)
            .ToListAsync();

        // Return the userPrompt if found; otherwise, return null
        return response.ToDto();
    }


    public async Task<List<UserPromptDto>> GetUserPromptsByDefinitionTypeAsync(string definitionType)
    {
        return await context.Chats
            .Where(c => c.DefinitionType == definitionType)
            .Select(c => c.ToDto())
            .ToListAsync();
    }
    public async Task<UserPromptDto> RefreshUserPromptResponses(UserPromptDto userPrompt)
    {
        // Find the userPromptId to update
        var userPromptDto = await FindResponseByUserPromptIdAsync(userPrompt.Id);
        if (userPromptDto == null)
            return new();

        // get the user userPromptId from the database
        var dbUserPrompt = await context.Chats
            .Include(r => r.GPTResponses).Where(w => w.DefinitionType == userPromptDto.DefinitionType)
            .FirstOrDefaultAsync(r => r.Id == userPrompt.Id);

        // Get list of all Definitions
        var definitions = await context.Definitions
            .Where(w => w.DefinitionType == dbUserPrompt.DefinitionType)
            .ToListAsync();
        // loop over all the dbDefinitions and update the responses
        foreach (var definition in definitions)
        {
            // Find the GPTUserPrompt for the definition
            var definitionResponse = dbUserPrompt
                .GPTResponses
                .FirstOrDefault(r => r.DefinitionId == definition.DefinitionId);
            if (definitionResponse == null)
            {
                definitionResponse = new GPTDefinitionResponse(definition, dbUserPrompt);
                definitionResponse = await UpdateGPTResponse(definitionResponse);
                dbUserPrompt.GPTResponses.Add(definitionResponse);
            }
            else
            {
                if (definitionResponse.UserPrompt != dbUserPrompt.UserPrompt)
                    definitionResponse.UserPrompt = dbUserPrompt.UserPrompt;

                if (definitionResponse.GPTName != definition.GPTName)
                    definitionResponse.GPTName = definition.GPTName;

                if (definitionResponse.Created < DateTime.UtcNow.AddDays(-10))
                    definitionResponse.Created = DateTime.UtcNow.AddDays(-5);

                if (definitionResponse.SystemResponse == "No Answer"
                    || definitionResponse.Updated.AddDays(1) < DateTime.UtcNow)
                {
                    definitionResponse = await UpdateGPTResponse(definitionResponse);
                }
            }
        }
        context.Chats.Update(dbUserPrompt);
        await context.SaveChangesAsync();
        return dbUserPrompt.ToDto();
    }
    public async Task<UserPromptDto> RefreshGPTResponse(UserPromptDto userPrompt)
    {
        // Find the userPromptId to update
        var userPromptDto = await FindResponseByUserPromptTextAsync(userPrompt.UserPrompt);
        if (userPromptDto == null)
            return userPrompt;

        // get the user userPromptId from the database
        var dbUserPrompt = await context.Chats
            .Include(r => r.GPTResponses)
            .Where(w => w.DefinitionType == userPromptDto.DefinitionType)
            .FirstOrDefaultAsync(r => r.UserPrompt == userPrompt.UserPrompt);

        if (dbUserPrompt == null)
            return userPrompt;

        // Get list of all Definitions
        var dbDefinitions = await context.Definitions
            .Where(w => w.DefinitionType == userPromptDto.DefinitionType)
            .ToListAsync();

        // loop over all the dbDefinitions and update the responses
        foreach (var definition in dbDefinitions)
        {
            // Find the GPTUserPrompt for the definition
            var definitionResponse = dbUserPrompt.GPTResponses.FirstOrDefault(r => r.DefinitionId == definition.DefinitionId);
            if (definitionResponse == null)
            {
                definitionResponse = new GPTDefinitionResponse(definition, dbUserPrompt);
                definitionResponse = await UpdateGPTResponse(definitionResponse);
                dbUserPrompt.GPTResponses.Add(definitionResponse);
            }
            else
            {
                if (definitionResponse.UserPrompt != dbUserPrompt.UserPrompt)
                    definitionResponse.UserPrompt = dbUserPrompt.UserPrompt;

                if (definitionResponse.GPTName != definition.GPTName)
                    definitionResponse.GPTName = definition.GPTName;

                if (definitionResponse.Created < DateTime.UtcNow.AddDays(-10))
                    definitionResponse.Created = DateTime.UtcNow.AddDays(-5);

                if (definitionResponse.SystemResponse == "No Answer"
                    || definitionResponse.Updated.AddDays(1) < DateTime.UtcNow)
                {
                    definitionResponse = await UpdateGPTResponse(definitionResponse);
                }
            }
        }
        try
        {
            context.Chats.Update(dbUserPrompt);
            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return userPrompt;
        }
        return dbUserPrompt.ToDto();
    }

    public async Task RerunAllPrompts()
    {
        var requests = await context.Chats
            .Select(c => c.ToDto())
            .ToListAsync();
        foreach (var req in requests)
        {
            await RefreshGPTResponse(req);
        }
    }
    public async Task RerunAllPrompts(int DefinitionId)
    {
        // Filter chats to include only those that have at least one response matching the specified DefinitionId
        var userPrompts = await context.Chats
            .Where(c => c.GPTResponses.Any(r => r.DefinitionId == DefinitionId))  // Ensure there's a matching DefinitionId in DefinitionResponses
            .Include(c => c.GPTResponses).ThenInclude(r => r.Definition) // Include the DefinitionResponses and their associated Definitions
            .Select(c => c)
            .ToListAsync();

        foreach (var userPrompt in userPrompts)
        {
            foreach (var res in userPrompt.GPTResponses)
            {
                if (res.DefinitionId != DefinitionId)
                    continue;

                if (res.GPTName != res.Definition.GPTName)
                    res.GPTName = res.Definition.GPTName;

                if (res.UserPrompt != userPrompt.UserPrompt)
                    res.UserPrompt = userPrompt.UserPrompt;

                await UpdateGPTResponse(res);
                context.DefinitionResponses.Update(res);
                context.SaveChanges();
            }
        }
    }
    public async Task<DefinitionResponseDto> UpdateGPTResponse(DefinitionResponseDto gptResponse)
    {
        var openAIRequest = GetOpenAiApiRequest(gptResponse);
        var serviceResponse = await httpClientService.PostAsync<OpenAiApiRequest, OpenAiApiResponse>(openAiUrl, openAIRequest, headers);
        gptResponse.SystemPrompt = openAIRequest.messages.Where(w => w.role == "system").FirstOrDefault()?.content;
        gptResponse.SystemResponse = serviceResponse?.Content?.Choices?.FirstOrDefault()?.Message?.content ?? "No Answer";
        gptResponse.Updated = serviceResponse?.CompletionDate ?? DateTime.UtcNow;
        gptResponse.TimeMS = serviceResponse?.ElapsedMilliseconds ?? 0;
        gptResponse.TotalTokens = serviceResponse?.Content?.usage?.total_tokens ?? 0;
        gptResponse.CompletionTokens = serviceResponse?.Content?.usage?.completion_tokens ?? 0;
        gptResponse.PromptTokens = serviceResponse?.Content?.usage?.prompt_tokens ?? 0;
        gptResponse.Model = serviceResponse?.Content?.Model ?? "Unknown";
        return gptResponse;
    }

    public async Task<GPTDefinitionResponse> UpdateGPTResponse(GPTDefinitionResponse gptResponse)
    {
        var openAIRequest = GetOpenAiApiRequest(gptResponse);
        var serviceResponse = await httpClientService.PostAsync<OpenAiApiRequest, OpenAiApiResponse>(openAiUrl, openAIRequest, headers);
        gptResponse.SystemPrompt = openAIRequest.messages.Where(w => w.role == "system").FirstOrDefault()?.content;
        gptResponse.SystemResponse = serviceResponse?.Content?.Choices?.FirstOrDefault()?.Message?.content ?? "No Answer";
        gptResponse.Updated = serviceResponse?.CompletionDate ?? DateTime.UtcNow;
        gptResponse.ElapsedMilliseconds = serviceResponse?.ElapsedMilliseconds ?? 0;
        gptResponse.TotalTokens = serviceResponse?.Content?.usage?.total_tokens ?? 0;
        gptResponse.CompletionTokens = serviceResponse?.Content?.usage?.completion_tokens ?? 0;
        gptResponse.PromptTokens = serviceResponse?.Content?.usage?.prompt_tokens ?? 0;
        gptResponse.Model = serviceResponse?.Content?.Model ?? "Unknown";
        return gptResponse;
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
    ~OpenAIChatCompletionService()
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
