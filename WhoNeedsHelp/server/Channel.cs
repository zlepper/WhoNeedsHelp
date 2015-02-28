using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace WhoNeedsHelp.server
{
    public class Channel
    {
        [Key]
        public Guid Id { get; set; }
        //public virtual List<Guid> Users { get; set; }
        //public virtual List<Guid> UsersRequestingHelp { get; set; }
        //public virtual List<Guid> Administrators { get; set; }
        public string Users { get; set; }
        public string UsersRequestingHelp { get; set; }
        public string Administrators { get; set; }
        public string ChannelName { get; set; }
        //public readonly List<string> ChatMessages = new List<string>();

        public Channel()
        {
        }

        public Channel(Guid userId)
        {
            //Users = new List<Guid>();
            //UsersRequestingHelp = new List<Guid>();
            //Administrators = new List<Guid> { userId };
            Users = "";
            UsersRequestingHelp = "";
            Administrators = "";
            AddAdministrator(userId);
            //Administrators.Add(userId);
            using (var db = new HelpContext())
            {
                Id = db.GenerateNewGuid(HelpContext.Modes.Channel);
            }
        }

        public void RemoveUserRequestingHelp(Guid u)
        {
            var usrh = Serialiser.DesiraliseGuidStringList(UsersRequestingHelp);
            usrh.Remove(u);
            UsersRequestingHelp = Serialiser.SerialiseList(usrh);
        }

        public List<Guid> GetUsers()
        {
            return Serialiser.DesiraliseGuidStringList(Users);
        }

        public List<Guid> GetUsersRequestingHelp()
        {
            return Serialiser.DesiraliseGuidStringList(UsersRequestingHelp);
        } 

        public bool IsUserAdministrator(Guid u)
        {
            return Administrators.Contains(u.ToString());
        }

        public void AddAdministrator(Guid u)
        {
            List<Guid> a = Serialiser.DesiraliseGuidStringList(Administrators);
            a.Add(u);
            Administrators = Serialiser.SerialiseList(a);
        }

        public int GetQuestingUserCount()
        {
            return Serialiser.DesiraliseGuidStringList(UsersRequestingHelp).Count;

        }

        public List<Guid> GetActiveUsers()
        {
            using (var db = new HelpContext())
            {
                return db.Users.Where(u => u.ChannelId.Equals(Id)).Select(user => user.Id).ToList();
            }
        }

        public bool RequestHelp(Guid user)
        {
            if (UsersRequestingHelp.Contains(user.ToString()))
            {
                return false;
            }
            List<Guid> l = Serialiser.DesiraliseGuidStringList(UsersRequestingHelp);
            l.Add(user);
            UsersRequestingHelp = Serialiser.SerialiseList(l);
            return true;
        }

        public bool AddUser(Guid u)
        {
            if (Users.Contains(u.ToString()))
            {
                return false;
            }
            List<Guid> l = Serialiser.DesiraliseGuidStringList(Users);
            l.Add(u);
            Users = Serialiser.SerialiseList(l);
            return true;
        }

        public void RemoveUser(Guid u)
        {
            if (Users.Contains(u.ToString()))
            {
                List<Guid> l = Serialiser.DesiraliseGuidStringList(Users);
                l.Remove(u);
                Users = Serialiser.SerialiseList(l);
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
                return message.Id;
            }
            return Guid.Empty;
        }

        public bool AppendMessageToLast(Guid messageId)
        {
            using (var db = new HelpContext())
            {
                var bunch = db.ChatMessages.Where(c => c.Channel.Equals(Id));
                var bunchArray = bunch.ToArray();
                if (bunchArray.Length > 1)
                {
                    //var stuffs = bunch.ToList();
                    ChatMessage lastChatMessage = bunchArray[bunchArray.Length - 2];
                    ChatMessage message = db.ChatMessages.Find(messageId);
                    if (lastChatMessage != null && message != null)
                    {
                        return lastChatMessage.Author.Equals(message.Author);
                    }
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