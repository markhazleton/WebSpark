using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PromptSpark.Chat.Services;

namespace PromptSpark.Chat.Controllers.Api;


[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly ConversationService _conversationService;
    private readonly IHubContext<PromptSparkHub> _hubContext;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(ConversationService conversationService, IHubContext<PromptSparkHub> hubContext, ILogger<ConversationsController> logger)
    {
        _conversationService = conversationService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpGet("{conversationId}", Name = "GetConversation")]
    public IActionResult GetConversation(string conversationId)
    {
        var conversation = _conversationService.Lookup(conversationId);
        if (conversation == null)
        {
            return NotFound();
        }

        var response = CreateHateoasResponse(conversation);
        return Ok(response);
    }

    [HttpPost]
    public IActionResult StartConversation()
    {
        var conversationId = Guid.NewGuid().ToString();
        var conversation = _conversationService.Lookup(conversationId);

        var response = CreateHateoasResponse(conversation);
        return CreatedAtRoute("GetConversation", new { conversationId = conversationId }, response);
    }

    [HttpPost("{conversationId}/sendMessage")]
    public async Task<IActionResult> SendMessage(string conversationId, [FromBody] string message, CancellationToken cancellationToken)
    {
        var conversation = _conversationService.Lookup(conversationId);
        if (conversation == null)
        {
            return NotFound();
        }

        var result = await _conversationService.ProcessSendMessage(message, conversationId, conversation, cancellationToken);
        if (result == null)
        {
            return BadRequest("Failed to process message.");
        }

        var response = CreateHateoasResponse(conversation, result.Value);
        return Ok(response);
    }

    [HttpPost("{conversationId}/respond")]
    public async Task<IActionResult> UserRespond(string conversationId, [FromBody] string userResponse, CancellationToken cancellationToken)
    {
        var conversation = _conversationService.Lookup(conversationId);
        if (conversation == null)
        {
            return NotFound();
        }

        // Replace `clientConnectionId` with the actual connection ID of the target client
        var specificClient = _hubContext.Clients.Client("clientConnectionId");
        if (specificClient == null)
        {
            return BadRequest("Client not found.");
        }

        var result = await _conversationService.ProcessUserResponse(conversationId, userResponse, conversation, specificClient, cancellationToken);

        if (result == null)
        {
            return BadRequest("Failed to process user response.");
        }

        var response = CreateHateoasResponse(conversation, result.Value);
        return Ok(response);
    }


    private object CreateHateoasResponse(Conversation conversation, (MessageType messageType, object messageData)? actionData = null)
    {
        var links = new List<object>
            {
                new { href = Url.Link("GetConversation", new { conversationId = conversation.ConversationId }), rel = "self", method = "GET" },
                new { href = Url.Action(nameof(SendMessage), new { conversationId = conversation.ConversationId }), rel = "send-message", method = "POST" },
                new { href = Url.Action(nameof(UserRespond), new { conversationId = conversation.ConversationId }), rel = "user-respond", method = "POST" }
            };

        return new
        {
            conversationId = conversation.ConversationId,
            userName = conversation.UserName,
            currentNodeId = conversation.CurrentNodeId,
            promptName = conversation.PromptName,
            chatHistory = conversation.ChatHistory,
            messageType = actionData?.messageType.ToString(),
            messageData = actionData?.messageData,
            _links = links
        };
    }
}

