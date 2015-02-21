using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class ChatMessage
    {
        public String Text { get; set; }
        public User Author { get; set; }
        public DateTime DateTime { get; set; }

        public ChatMessage(string text, User user, DateTime dt)
        {
            this.Text = text;
            this.Author = user;
            this.DateTime = dt;
        }

        public ChatMessage(string text, User user)
        {
            this.Text = text;
            this.Author = user;
            this.DateTime = DateTime.Now;
        }

        public string GetMessageId()
        {
            return Author.ConnectionId + "-" + DateTime.Millisecond;
        }
    }
}