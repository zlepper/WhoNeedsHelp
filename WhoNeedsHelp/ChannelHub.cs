using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace WhoNeedsHelp
{
    public class ChannelHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}