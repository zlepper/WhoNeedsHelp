using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoNeedsHelp.Simples
{
    public class SimpleChatMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public SimpleUser User { get; set; }

        public SimpleChatMessage(int id, string text, SimpleUser user)
        {
            Id = id;
            Text = text;
            User = user;
        }
    }
}
