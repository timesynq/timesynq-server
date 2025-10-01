using Microsoft.AspNetCore.SignalR;

namespace TimesynqServer.Hubs.RefreshHub
{
    public class RefreshHub : Hub
    {
        public RefreshHub() { }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
