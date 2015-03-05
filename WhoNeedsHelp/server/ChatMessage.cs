using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WhoNeedsHelp.server;

namespace WhoNeedsHelp
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int ChannelId { get; set; }
        public Channel Channel { get; set; }
        public DateTime Time { get; set; }

        public ChatMessage() { }

        public ChatMessage(string text, User user, Channel channel)
        {
            Text = text;
            User = user;
            Channel = channel;
            Time = DateTime.Now;
        }
    }
}