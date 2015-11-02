using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using WhoNeedsHelp.Models;
using WhoNeedsHelp.Server.Chat;

namespace WhoNeedsHelp.Simples
{
    public class SimpleChannel
    {
        public int Id { get; set; }
        public string ChannelName { get; set; }
        public Dictionary<int, SimpleQuestion> Questions { get; set; }
        public Dictionary<int, SimpleChatMessage> ChatMessages { get; set; }
        public Dictionary<int, SimpleUser> Users { get; set; }
        public bool IsAdmin { get; set; }
        public int TimeLeft { get; set; }

        public SimpleChannel(int id, bool isAdmin = false)
        {
            IsAdmin = isAdmin;
            using (HelpContext db = new HelpContext())
            {
                Channel c = db.Channels.Find(id);
                Id = id;
                ChannelName = c.ChannelName;
                Questions = new Dictionary<int, SimpleQuestion>();
                foreach (Question q in c.Questions)
                {
                    Questions.Add(q.Id, q.ToSimpleQuestion());
                }
                ChatMessages = new Dictionary<int, SimpleChatMessage>();
                foreach (ChatMessage cm in c.ChatMessages)
                {
                    ChatMessages.Add(cm.Id, cm.ToSimpleChatMessage());
                }
                Users = new Dictionary<int, SimpleUser>();
                foreach (User u in c.Users)
                {
                    Users.Add(u.Id, u.ToSimpleUser());
                }
                if (IsAdmin)
                    TimeLeft = c.TimeLeft;
            }
        }
    }
}
