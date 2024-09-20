using Microsoft.AspNet.SignalR;

namespace AngularJSAuthentication.API.SignalR
{
    public class ChatFeed : SignalRBase<ChatHub>
    {
        public static void SendChatMessage(string message, string groupName)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            context.Clients.All.broadcastMessage(message);
            context.Clients.Group(groupName).addChatMessage(message);
        }
    }
}