using System;
using System.Collections.Generic;
using System.Linq;
using WhoNeedsHelp.Server.Chat;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.Models
{
    public class User
    {
        protected bool Equals(User other)
        {
            return Id == other.Id && string.Equals(EmailAddress, other.EmailAddress);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (EmailAddress?.GetHashCode() ?? 0);
            }
        }

        public int Id { get; set; }

        public string VirtualId { get; set; }

        //public ICollection<Connection> Connections { get; set; } 
        public string Name { get; set; }
        public string Pw { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Channel> ChannelsIn { get; set; }
        public virtual ICollection<Channel> ChannelsRequestingHelpIn { get; set; }
        public virtual ICollection<Channel> AreAdministratorIn { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
        public virtual ICollection<LoginToken> LoginTokens { get; set; } 
        public string Ip { get; set; }
        public virtual ICollection<Connection> Connections { get; set; } 
        public string EmailAddress { get; set; }

        public int FailedLoginAttempts { get; set; }
        public DateTime LastFailedAttempt { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public DateTime ResetExpiresAt { get; set; }
        public string ResetKey { get; set; }

        public int? PreferedLocaleId { get; set; }
        public virtual Locale PreferedLocale { get; set; }

        public virtual ICollection<CleanupAlarm> CleanupAlarms { get; set; } 

        public User()
        {
            Questions = new List<Question>();
            Connections = new List<Connection>();
            LastLogin = DateTime.Now;
            CreatedAt = DateTime.Now;
        }

        public Question RequestHelp(Channel ch, string question = null)
        {
            if (ch != null)
            {
                using (HelpContext db = new HelpContext())
                {
                    bool help = ch.RequestHelp(this);
                    if (!help) return null;
                    if (AreUserQuestioning(ch))
                        return null;
                    Question q = new Question(ch, question, this);
                    return q;
                }
            }
            return null;
        }

        public Question GetQuestion(Channel c)
        {
            return Questions.SingleOrDefault(q => q.Channel.Equals(c));
        }

        public Channel GetChannel(int channelId)
        {
            return ChannelsIn.SingleOrDefault(c => c.Id == channelId);
        }

        /// <summary>
        /// Updates the question for the selected channel
        /// </summary>
        /// <param name="c">The channel Guid to change question in</param>
        /// <param name="question">The question to change to</param>
        /// <returns>Returns the question if it was edited</returns>
        public Question UpdateQuestion(Channel c, string question)
        {
            Question q = Questions.SingleOrDefault(qu => qu.Channel.Equals(c));
            if (q == null) return null;
            q.Text = question;
            return q;
        }

        public bool AreUserQuestioning(Channel c)
        {
            if (c == null) return false;
            var question = Questions.SingleOrDefault(q => q.Channel.Equals(c));
            return question != null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((User) obj);
        }

        public SimpleUser ToSimpleUser()
        {
            return new SimpleUser(Id, Name);
        }

        public void GenerateLoginToken(Guid key)
        {
            LoginTokens.Add(new LoginToken(this, key));
        }

        public LoginToken CheckLoginToken(string key)
        {
            LoginToken loginToken = LoginTokens.SingleOrDefault(token => PasswordHash.ValidatePassword(key, token.Key));
            return loginToken;
        }

        public string GenerateResetKey()
        {
            ResetExpiresAt = DateTime.Now.AddHours(2);
            string key = Guid.NewGuid().ToString();
            ResetKey = PasswordHash.CreateHash(key);
            return key;
        }

        public bool CanPasswordBeReset(string key)
        {
            return ResetExpiresAt > DateTime.Now && PasswordHash.ValidatePassword(key, ResetKey);
        }
    }
}