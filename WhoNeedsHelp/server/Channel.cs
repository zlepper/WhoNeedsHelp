using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WhoNeedsHelp.server
{
    public class Channel
    {
        [Key]
        public Guid Id { get; set; }
        public readonly List<Guid> Users = new List<Guid>();
        public readonly List<Guid> UsersRequestingHelp = new List<Guid>();
        private readonly List<Guid> _administrators = new List<Guid>();
        public string ChannelName { get; set; }
        //public readonly List<string> ChatMessages = new List<string>();

        public Channel() { }

        public Channel(Guid userId)
        {
            _administrators.Add(userId);
            using (var db = new HelpContext())
            {
                Id = db.GenerateNewGuid(HelpContext.Modes.Channel);
            }
        }

        public bool IsUserAdministrator(Guid u)
        {
            return _administrators.Contains(u);
        }

        public void AddAdministrator(Guid u)
        {
            _administrators.Add(u);
        }

        public int GetActiveUserCount()
        {
            return UsersRequestingHelp.Count;
        }

        public List<Guid> GetActiveUsers()
        {
            return UsersRequestingHelp;
        }

        public bool RequestHelp(Guid user)
        {
            if (UsersRequestingHelp.Contains(user))
            {
                return false;
            }
            UsersRequestingHelp.Add(user);
            return true;

        }

        public bool AddUser(Guid u)
        {
            if (Users.Contains(u))
            {
                return false;
            }
            Users.Add(u);
            return true;
        }

        public void RemoveUser(Guid u)
        {
            if (Users.Contains(u))
            {
                Users.Remove(u);
            }
        }

        /// <summary>
        /// Adds the chat message to the channel
        /// </summary>
        /// <param name="author">The Guid of the author user</param>
        /// <param name="text">The text in the message</param>
        /// <returns>The Guid of the new message</returns>
        public Guid AddChatMessage(Guid author, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                ChatMessage message = new ChatMessage(text, author, Id);
                using (var db = new HelpContext())
                {
                    db.ChatMessages.Add(message);
                    db.SaveChanges();
                }
                return message.MessageId;
            }
            return Guid.Empty;
        }

        public bool AppendMessageToLast(Guid messageId)
        {
            /*if (ChatMessages.Count > 1)
            {
                var lastChatMessage = ChatMessages.Values.ToArray()[ChatMessages.Count - 2];
                return lastChatMessage.Author == messageId.Author;
            }
            return false;*/
            using (var db = new HelpContext())
            {
                ChatMessage lastChatMessage = db.ChatMessages.Where(c => c.Channel == Id).Reverse().Skip(1).Take(1).SingleOrDefault();
                ChatMessage message = db.ChatMessages.Find(messageId);
                if (lastChatMessage != null && message != null)
                {
                    return lastChatMessage.Author.Equals(message.Author);
                }
            }
            return false;

        }

        /*public bool AppendMessageToLast(int index, User author)
        {
            if (index < 1)
            {
                return false;
            }
            ChatMessage previousMessage = ChatMessages.Values.ToArray()[index - 1];
            return previousMessage.Author == author;
            if (index < 1)
            {
                return false;
            }
            using (var db = new HelpContext())
            {
                ChatMessage lastChatMessage = db.ChatMessages.Where(c => c.Channel == Id).Reverse().Skip(1).Take(1).SingleOrDefault();
                ChatMessage message = db.ChatMessages.Find(messageId);
                if (lastChatMessage != null && message != null)
                {
                    return lastChatMessage.Author.Equals(message.Author);
                }
            }
            return false;
        }*/
    }
}