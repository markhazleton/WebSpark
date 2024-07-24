using Markdig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using PromptSpark.Utilities;
using System.Text;

namespace PromptSpark.Areas.OpenAI.Controllers;


public class ChatCompletionController(
    IHttpContextAccessor _httpContextAccessor,
    IHubContext<ChatHub> hubContext,
    IChatCompletionService _chatCompletionService,
    IGPTDefinitionService definitionService,
    ILogger<ChatCompletionController> logger) : OpenAIBaseController
{
    public async Task<IActionResult> Index(int id = 0)
    {
        if (id == 0) Response.Redirect("/OpenAI/Chat");
        var definitionDto = await definitionService.GetDefinitionDtoAsync(id);
        definitionDto.ConversationId = Guid.NewGuid();

        var session = _httpContextAccessor.HttpContext.Session;
        session.SetString("DefinitionDto", JsonConvert.SerializeObject(definitionDto));


        return View(definitionDto);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromForm] string message, [FromForm] string conversationHistory)
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var definitionDtoJson = session.GetString("DefinitionDto");
        var definitionDto = JsonConvert.DeserializeObject<DefinitionDto>(definitionDtoJson);

        if (!string.IsNullOrEmpty(message) && definitionDto != null)
        {
            ChatHistory chatHistory = GetChatHistory(message, conversationHistory, definitionDto);

            await ProcessChatHistory(chatHistory);
            return Ok();
        }
        logger.LogError("Invalid input");
        return BadRequest("Invalid input");
    }

    private ChatHistory GetChatHistory(string message, string conversationHistory, DefinitionDto? definitionDto)
    {
        logger.LogWarning("Received message: {message} for {Name}", message, definitionDto.Name);
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(definitionDto.Prompt);
        chatHistory.AddSystemMessage("You are in a conversation, keep your answers brief, always ask follow-up questions, ask if ready for full answer.");

        // Parse the conversationHistory JSON string into a list of messages
        var messages = JsonConvert.DeserializeObject<List<string>>(conversationHistory);

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

        return chatHistory;
    }

    private async Task ProcessChatHistory(
        ChatHistory chatHistory)
    {
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
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing the request");
        }
    }
}
