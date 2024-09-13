using Markdig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

public class ChatCompletionController(
    IHttpContextAccessor _httpContextAccessor,
    IHubContext<ChatHub> hubContext,
    IChatCompletionService _chatCompletionService,
    IGPTDefinitionService definitionService,
    ILogger<ChatCompletionController> logger) : PromptSparkBaseController
{
    public async Task<IActionResult> Index(int id = 0)
    {
        logger.LogInformation("Entering Index method with id: {Id}", id);
        try
        {
            if (id == 0)
            {
                // Get list of definitions
                var definitions = await definitionService.GetDefinitionsAsync();
                logger.LogInformation("Retrieved {Count} definitions.", definitions.Count);

                // Return to pick definition view
                return View("PickDefinition", definitions);
            }

            var definitionDto = await definitionService.GetDefinitionDtoAsync(id);
            if (definitionDto == null)
            {
                logger.LogWarning("No definition found for id: {Id}", id);
                return NotFound("Definition not found");
            }

            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString("DefinitionDto", JsonConvert.SerializeObject(definitionDto));
            logger.LogInformation("Definition stored in session for id: {Id}", id);

            return View(definitionDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing Index with id: {Id}", id);
            return View("Error"); // Return a user-friendly error view
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] string message, [FromForm] string conversationHistory)
    {
        logger.LogInformation("Entering SendMessage method with message: {Message} and conversation history length: {Length}", message, conversationHistory?.Length);

        if (string.IsNullOrEmpty(message))
        {
            logger.LogWarning("Received empty message.");
            return BadRequest("Message cannot be empty");
        }

        var session = _httpContextAccessor.HttpContext.Session;
        var definitionDtoJson = session.GetString("DefinitionDto");

        if (string.IsNullOrEmpty(definitionDtoJson))
        {
            logger.LogWarning("DefinitionDto not found in session.");
            return BadRequest("Session expired or invalid");
        }

        var definitionDto = JsonConvert.DeserializeObject<DefinitionDto>(definitionDtoJson);
        if (definitionDto == null)
        {
            logger.LogError("Failed to deserialize DefinitionDto from session.");
            return BadRequest("Invalid session data");
        }

        logger.LogInformation("Processing message for definition: {Name}", definitionDto.Name);

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(definitionDto.Prompt);
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions, ask if ready for full answer.");

        // Parse the conversationHistory JSON string into a list of messages
        List<string> messages;
        try
        {
            messages = JsonConvert.DeserializeObject<List<string>>(conversationHistory);
            logger.LogInformation("Parsed conversation history with {Count} messages.", messages?.Count ?? 0);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse conversation history JSON.");
            return BadRequest("Invalid conversation history format");
        }

        // Populate ChatHistory with messages from conversationHistory
        for (int i = 0; i < messages.Count; i++)
        {
            if (i % 2 == 0)
            {
                chatHistory.AddUserMessage(messages[i]); // Even index - User
            }
            else
            {
                chatHistory.AddSystemMessage(messages[i]); // Odd index - System
            }
        }

        try
        {
            var buffer = new StringBuilder();

            await foreach (var response in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
            {
                if (response?.Content != null)
                {
                    buffer.Append(response.Content);
                    if (response.Content.Contains('\n'))
                    {
                        var contentToSend = buffer.ToString();
                        var htmlContent = Markdown.ToHtml(contentToSend);
                        await hubContext.Clients.All.SendAsync("ReceiveMessage", "System", htmlContent);
                        logger.LogInformation("Sent message to client: {Message}", contentToSend);
                        buffer.Clear();
                    }
                }
            }

            // If there's any remaining content in the buffer, send it
            if (buffer.Length > 0)
            {
                var remainingContent = buffer.ToString();
                var htmlContent = Markdown.ToHtml(remainingContent);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", "System", htmlContent);
                logger.LogInformation("Sent remaining content to client: {RemainingContent}", remainingContent);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing the chat completion request.");
            return StatusCode(500, "An error occurred while processing your request.");
        }

        logger.LogInformation("SendMessage completed successfully.");
        return Ok();
    }
}
