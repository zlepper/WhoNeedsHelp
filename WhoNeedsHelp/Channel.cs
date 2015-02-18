using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class Channel
    {
        public readonly Dictionary<string, User> Users = new Dictionary<string, User>();
        private readonly List<User> _usersRequestingHelp = new List<User>();
        public readonly User Administrator;
        public string ChannelName;
        public string ChannelId;

        public Channel(User u)
        {
            Administrator = u;
        }

        public int GetActiveUsers()
        {
            return Users.Values.Count(user => user.CurrentChannel == this);
        }

        public bool RequestHelp(User user)
        {
            if (_usersRequestingHelp.Contains(user))
            {
                return false;
            }
            _usersRequestingHelp.Add(user);
            return true;
        }

        public string CreateTable()
        {
            string table = "";
            foreach (User u in _usersRequestingHelp)
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
    }
}