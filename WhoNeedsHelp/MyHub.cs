using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace WhoNeedsHelp
{
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping<string> _Connections = new ConnectionMapping<string>(); 
        private readonly static List<string[]> ChatList = new List<string[]>(); 

        public void Hello()
        {
            Clients.All.hello();
        }

        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            ChatList.Add(new string[2] {name, message});
            Clients.All.broadcastMessage(name, message);
        }

        public void GetData()
        {
            String s = ChatList.Aggregate("", (current, value) => current + String.Format("<li><strong>{0}</strong>:&nbsp;&nbsp;{1}</li>", value[0], value[1]));
            Clients.Caller.sendOldData(s);

        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;

            _Connections.Add(name, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;

            _Connections.Remove(name, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;

            if (!_Connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _Connections.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}