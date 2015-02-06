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
        public User Administrator;
        public string ChannelName;
        public string ChannelId;

        public bool RequestHelp(User user)
        {
            if (UsersRequestingHelp.Contains(user))
            {
                return false;
            }
            UsersRequestingHelp.Add(user);
            return true;
        }

        public string CreateHtml()
        {
            string html = string.Format("<a href='#' id='{1}' class='list-group-item'>{0}</a>", ChannelName,
                ChannelId + "-selector");
            return html;
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
            if (!Users.ContainsKey(u.ConnectionId))
            {
                Users.Add(u.ConnectionId, u);
            }
        }
    }
}