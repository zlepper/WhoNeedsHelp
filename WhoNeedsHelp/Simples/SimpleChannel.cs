﻿using System.Collections.Generic;
using System.Linq;
using WhoNeedsHelp.server;

namespace WhoNeedsHelp.Simples
{
    public class SimpleChannel
    {
        public int Id { get; set; }
        public string ChannelName { get; set; }
        public Dictionary<int, SimpleQuestion> Questions { get; set; }
        public Dictionary<int, SimpleChatMessage> ChatMessages { get; set; }
        public Dictionary<int, SimpleUser> Users { get; set; }
        public bool HasAdmin { get; set; }

        public SimpleChannel(int id, bool hasAdmin = false)
        {
            HasAdmin = hasAdmin;
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
            }
        }
    }
}
