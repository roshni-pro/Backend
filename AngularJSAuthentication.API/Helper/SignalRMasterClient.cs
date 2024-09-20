using Microsoft.AspNet.SignalR.Client;
using System;

namespace AngularJSAuthentication.API.Helper
{
    public class SignalRMasterClient
    {
        public string Url { get; set; }
        public HubConnection Connection { get; set; }
        public IHubProxy Hub { get; set; }

        public SignalRMasterClient(string url)
        {
            Url = url;
            Connection = new HubConnection(url, useDefaultUrl: false);
            Hub = Connection.CreateHubProxy("chatHub");
            Connection.Start().Wait();

            Hub.On<string>("acknowledgeMessage", (message) =>
            {
                Console.WriteLine("Message received: " + message);

                /// TODO: Check status of the LDAP
                /// and update status to Web API.
            });
        }

        public void SayHello(string message, string groupName)
        {
            Hub.Invoke("send", message, groupName);
            //Hub.Invoke("SendToGroup", message, groupName);
        }

        public void Stop()
        {
            Connection.Stop();
        }
    }
}