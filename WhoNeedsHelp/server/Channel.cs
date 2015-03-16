﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WhoNeedsHelp.server
{
    public class Channel
    {
        protected bool Equals(Channel other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        //[Key]
        public int Id { get; set; }
        //[InverseProperty("ChannelsIn")]
        public virtual ICollection<User> Users { get; set; }
        //[InverseProperty("ChannelsRequestingHelpIn")]
        public virtual ICollection<User> UsersRequestingHelp { get; set; }
        //[InverseProperty("AreAdministratorIn")]
        public virtual ICollection<User> Administrators { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<User> ActiveUsers { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }

        public string ChannelName { get; set; }

        public Channel()
        {
            /*Users = new List<User>();
            UsersRequestingHelp = new List<User>();
            Administrators = new List<User>();*/
        }

        public Channel(User user, string channelName)
        {
            ChannelName = channelName;
            Users = new List<User>();
            UsersRequestingHelp = new List<User>();
            Administrators = new List<User>();
            ActiveUsers = new List<User>();
            AddAdministrator(user);
        }

        public bool HasIp(string ip)
        {
            return Users.Count(u => u.Ip.Equals(ip)) > 0;
        }

        public void RemoveUserRequestingHelp(User u)
        {
            UsersRequestingHelp.Remove(u);
            return;
        }

        public List<User> GetUsers()
        {
            return Users.ToList();
        }

        public List<User> GetUsersRequestingHelp()
        {
            return UsersRequestingHelp.ToList();
        } 

        public bool IsUserAdministrator(User u)
        {
            return Administrators.Contains(u);
        }

        public void AddAdministrator(User u)
        {
            Administrators.Add(u);
        }

        public int GetQuestingUserCount()
        {
            return UsersRequestingHelp.Count;
        }

        public IEnumerable<User> GetActiveUsers()
        {
            return ActiveUsers;
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

        public bool AddUser(User u)
        {
            if (Users.Contains(u))
            {
                return false;
            }
            Users.Add(u);
            return true;
        }

        public void RemoveUser(User u)
        {
            if (Users.Contains(u))
            {
                Users.Remove(u);

            }
            if (UsersRequestingHelp.Contains(u))
            {
                UsersRequestingHelp.Remove(u);
            }
            if (ActiveUsers.Contains(u))
            {
                ActiveUsers.Remove(u);
            } 
        }

        /// <summary>
        /// Adds the chat message to the channel
        /// </summary>
        /// <param name="author">The Guid of the author user</param>
        /// <param name="text">The text in the message</param>
        /// <returns>The Guid of the new message</returns>
        public ChatMessage AddChatMessage(User author, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                ChatMessage message = new ChatMessage(text, author, this);
                return message;
            }
            return null;
        }

        public bool AppendMessageToLast(ChatMessage chatMessage)
        {
            return false;
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(null, o)) return false;
            if (ReferenceEquals(this, o)) return true;
            if (o.GetType() != this.GetType()) return false;
            return Equals((Channel) o);
        }
    }
}