using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebSpark.Portal.Utilities;

namespace WebSpark.Portal.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiagnosticsController : ControllerBase
{
    private readonly IHubContext<ChatHub> _chatHubContext;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(IHubContext<ChatHub> chatHubContext, ILogger<DiagnosticsController> logger)
    {
        _chatHubContext = chatHubContext;
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }

    [HttpPost("test-signalr")]
    public async Task<IActionResult> TestSignalR([FromQuery] string message = "Test message from diagnostics")
    {
        try
        {
            _logger.LogInformation("Testing SignalR broadcast with message: {Message}", message);
            // Send a test message to all connected clients
            await _chatHubContext.Clients.All.SendAsync("ReceiveMessage", "System",
                $"<p>Test message from server: {message}</p>",
                "test-conversation",
                $"test-{Guid.NewGuid()}",
                false);

            return Ok(new
            {
                success = true,
                message = "Test message sent to all SignalR clients",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SignalR");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
