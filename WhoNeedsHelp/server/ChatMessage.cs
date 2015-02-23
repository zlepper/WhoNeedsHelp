using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public User Author { get; set; }
        public DateTime DateTime { get; set; }
        public string MessageId { get; set; }

        public ChatMessage(string text, User user, DateTime dt)
        {
            this.Text = text;
            this.Author = user;
            this.DateTime = dt;
            MessageId = Author.ConnectionId + "-" + DateTime.Millisecond;
        }

        public ChatMessage(string text, User user)
        {
            this.Text = text;
            this.Author = user;
            this.DateTime = DateTime.Now;
            MessageId = Author.ConnectionId + "-" + DateTime.Millisecond;
        }

        public string GetMessageId()
        {
            return MessageId;
        }
    }
}