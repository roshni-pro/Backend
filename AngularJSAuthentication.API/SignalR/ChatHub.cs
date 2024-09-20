using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace AngularJSAuthentication.API.SignalR
{

    public class ChatHub : Hub
    {
        public void Send(string dboyCurrentLocation, string groupName)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(dboyCurrentLocation);
            Clients.Group(groupName).addChatMessage(dboyCurrentLocation);
        }


        public void SendToGroup(string dboyCurrentLocation, string groupName)
        {
            Clients.Group(groupName).addChatMessage(dboyCurrentLocation);
        }

        public async Task AddToGroup(string warehouseName)
        {
            await this.Groups.Add(Context.ConnectionId, warehouseName);
        }


        public async Task LeaveGroupAsync(string roomName)
        {
            await Groups.Remove(Context.ConnectionId, roomName);
        }
    }
}