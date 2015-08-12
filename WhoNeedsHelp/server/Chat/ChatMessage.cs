using System;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.Server.Chat
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; }
        public DateTime Time { get; set; }

        public ChatMessage() { }

        public ChatMessage(string text, User user, Channel channel)
        {
            Text = text;
            User = user;
            Channel = channel;
            Time = DateTime.Now;
        }

        public SimpleChatMessage ToSimpleChatMessage()
        {
            return new SimpleChatMessage(this.Id, this.Text, User.ToSimpleUser());
        }
    }
}