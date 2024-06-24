using Microsoft.AspNetCore.SignalR;

namespace WebSpark.Admin.Utilities;
public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
