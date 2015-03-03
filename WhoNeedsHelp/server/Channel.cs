using System;
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
            //Users = "";
            //UsersRequestingHelp = "";
            //Administrators = "";
            AddAdministrator(user);
            //Administrators.Add(userId);
        }

        public void RemoveUserRequestingHelp(User u)
        {
            /*var usrh = Serialiser.DesiraliseGuidStringList(UsersRequestingHelp);
            usrh.Remove(u);
            UsersRequestingHelp = Serialiser.SerialiseList(usrh);*/
            UsersRequestingHelp.Remove(u);
        }

        public List<User> GetUsers()
        {
            //return Serialiser.DesiraliseGuidStringList(Users);
            return Users.ToList();
        }

        public List<User> GetUsersRequestingHelp()
        {
            //return Serialiser.DesiraliseGuidStringList(UsersRequestingHelp);
            return UsersRequestingHelp.ToList();
        } 

        public bool IsUserAdministrator(User u)
        {
            return Administrators.Contains(u);
        }

        public void AddAdministrator(User u)
        {
            //List<int> a = Serialiser.DesiraliseGuidStringList(Administrators);
            //a.Add(u);
            //Administrators = Serialiser.SerialiseList(a);
            Administrators.Add(u);
        }

        public int GetQuestingUserCount()
        {
            //return Serialiser.DesiraliseGuidStringList(UsersRequestingHelp).Count;
            return UsersRequestingHelp.Count;
        }

        public List<User> GetActiveUsers()
        {
            using (var db = new HelpContext())
            {
                return db.Users.Where(u => u.Channel.Equals(this)).ToList();
            }

        }

        public bool RequestHelp(User user)
        {
            if (UsersRequestingHelp.Contains(user))
            {
                return false;
            }
            //List<int> l = Serialiser.DesiraliseGuidStringList(UsersRequestingHelp);
            //l.Add(user);
            //UsersRequestingHelp = Serialiser.SerialiseList(l);
            UsersRequestingHelp.Add(user);
            return true;
        }

        public bool AddUser(User u)
        {
            if (Users.Contains(u))
            {
                return false;
            }
            //List<int> l = Serialiser.DesiraliseGuidStringList(Users);
            //l.Add(u);
            //Users = Serialiser.SerialiseList(l);
            Users.Add(u);
            return true;
        }

        public void RemoveUser(User u)
        {
            if (Users.Contains(u))
            {
                //List<int> l = Serialiser.DesiraliseGuidStringList(Users);
                //l.Remove(u);
                //Users = Serialiser.SerialiseList(l);
                Users.Remove(u);
            }
        }

        /// <summary>
        /// Adds the chat message to the channel
        /// </summary>
        /// <param name="author">The Guid of the author user</param>
        /// <param name="text">The text in the message</param>
        /// <returns>The Guid of the new message</returns>
        public int AddChatMessage(User author, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                ChatMessage message = new ChatMessage(text, author, this);
                using (var db = new HelpContext())
                {
                    db.ChatMessages.Add(message);
                    db.SaveChanges();
                }
                return message.Id;
            }
            return -1;
        }

        public bool AppendMessageToLast(ChatMessage chatMessage)
        {
            using (var db = new HelpContext())
            {
                var bunch = db.ChatMessages.Where(cm => cm.Channel.Equals(this));
                var bunchArray = bunch.ToArray();
                if (bunchArray.Length > 1)
                {
                    //var stuffs = bunch.ToList();
                    ChatMessage lastChatMessage = bunchArray[bunchArray.Length - 2];
                    ChatMessage message = db.ChatMessages.Find(chatMessage);
                    if (lastChatMessage != null && message != null)
                    {
                        return lastChatMessage.User.Equals(message.User);
                    }
                }
            }
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