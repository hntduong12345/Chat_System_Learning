using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public ChatHub()
        {
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinSession(string sessionId)
        {

        }

        public async Task LeaveSession(string sessionId)
        {

        }

        public async Task SendMessage(string sessionId, string content)
        {

        }

        public async Task MarkMessagesAsRead(string sessionId)
        {

        }
    }
}
