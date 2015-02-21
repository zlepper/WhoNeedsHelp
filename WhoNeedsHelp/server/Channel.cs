using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class Channel
    {
        public readonly Dictionary<string, User> Users = new Dictionary<string, User>();
        public readonly List<User> UsersRequestingHelp = new List<User>();
        public readonly User Administrator;
        public string ChannelName;
        public string ChannelId;
        public List<ChatMessage> ChatMessages = new List<ChatMessage>(); 

        public Channel(User u)
        {
            Administrator = u;
        }

        public int GetActiveUserCount()
        {
            return Users.Values.Count(user => user.CurrentChannel == this);
        }

        public List<User> GetActiveUsers()
        {
            return Users.Values.Where(user => user.CurrentChannel == this).ToList();
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
                ChatMessages.Add(message);
                return message;
            }
            return null;
        }

        public bool appendMessageToLast(ChatMessage message)
        {
            if (ChatMessages.Count > 1)
            {
                ChatMessage lastChatMessage = ChatMessages[ChatMessages.Count - 2];
                return lastChatMessage.Author == message.Author;
            }
            return false;
        }
    }
}