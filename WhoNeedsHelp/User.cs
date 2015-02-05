using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class User
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Channel CurrentChannel { get; set; }
        public List<Channel> ConnectChannels { get; set; }
        private Dictionary<Channel, string> Questions { get; set; } 

        public bool RequestHelp()
        {
            return CurrentChannel.RequestHelp(this);
        }

        public string GetQuestion(Channel c)
        {
            return Questions[c];
        }
    }
}