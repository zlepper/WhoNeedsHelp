using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WhoNeedsHelp
{
    public class HelpHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        // TODO Add counter to specific rooms

        private void RequestTable(string channel)
        {
            // TODO finish function
        }

        private string createTable(string channel)
        {
            // TODO figure out how table should look
            return null;
        }

        private void RequestHelp(string channel, string name)
        {
            // TODO request help
        }

        public void Send(int action, string channel)
        {
            
        }

    }
}