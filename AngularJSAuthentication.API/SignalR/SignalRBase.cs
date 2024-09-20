using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;

namespace AngularJSAuthentication.API.SignalR
{
    public abstract class SignalRBase<T> where T : IHub
    {

        private Lazy<IHubContext> hub = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<T>()
        );

        public IHubContext Hub
        {
            get { return hub.Value; }
        }
    }
}