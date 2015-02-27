using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class Channel
    {
        public readonly Dictionary<string, User> Users = new Dictionary<string, User>();
        public readonly List<User> UsersRequestingHelp = new List<User>();
        public User Administrator { get; set; }
        public string ChannelName { get; set; }

        [Key]
        public string ChannelId { get; set; }
        public readonly  Dictionary<string, ChatMessage> ChatMessages = new Dictionary<string, ChatMessage>(); 
        //public readonly List<ChatMessage> ChatMessages = new List<ChatMessage>(); 

        public Channel(User u)
        {
            Administrator = u;
        }

        public int GetActiveUserCount()
        {
            return Users.Values.Count(user => user.Channel == this);
        }

        public List<User> GetActiveUsers()
        {
            return Users.Values.Where(user => user.Channel == this).ToList();
        }

        public bool RequestHelp(User user)
        {
            if (UsersRequestingHelp.Contains(user))
            {
                return false;
            }
            UsersRequestingHelp.Add(user);
            return true;
        }

        public string CreateTable()
        {
            string table = "";
            foreach (User u in UsersRequestingHelp)
            {
                string question = u.GetQuestion(this);
                table +=
                    String.Format(
                        "<div class='panel panel-primary'><div class='panel-heading'><h3 class='panel-title'>{0}</h3>" +
                        "</div><div class='panel-body'>{1}</div></div>", u.Name, question);
            }
            return table;
        }

        public List<User> GetUsers()
        {
            return Users.Select(u => u.Value).ToList();
        }

        public bool AddUser(User u)
        {
            if (Users.ContainsKey(u.ConnectionId))
            {
                return false;
            }
            Users.Add(u.ConnectionId, u);
            return true;
        }

        public void RemoveUser(User u)
        {
            if (Users.ContainsKey(u.ConnectionId))
            {
                Users.Remove(u.ConnectionId);
            }
        }

        public ChatMessage AddChatMessage(User author, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                ChatMessage message = new ChatMessage(text, author);
                ChatMessages.Add(message.MessageId, message);
                return message;
            }
            return null;
        }

        public bool AppendMessageToLast(ChatMessage message)
        {
            if (ChatMessages.Count > 1)
            {
                var lastChatMessage = ChatMessages.Values.ToArray()[ChatMessages.Count - 2];
                return lastChatMessage.Author == message.Author;
            }
            return false;
        }

        public bool AppendMessageToLast(int index, User author)
        {
            if (index < 1)
            {
                return false;
            }
            ChatMessage previousMessage = ChatMessages.Values.ToArray()[index - 1];
            return previousMessage.Author == author;
        }
    }
}