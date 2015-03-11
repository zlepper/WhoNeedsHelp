using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data;
using System.Data.Entity;

namespace WhoNeedsHelp.server
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
                return (Id * 397) ^ (EmailAddress != null ? EmailAddress.GetHashCode() : 0);
            }
        }

        public int Id { get; set; }

        //public ICollection<Connection> Connections { get; set; } 
        public string Name { get; set; }
        public int? ChannelId { get; set; }
        public string Pw { get; set; }
        public Channel Channel { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Channel> ChannelsIn { get; set; }
        public virtual ICollection<Channel> ChannelsRequestingHelpIn { get; set; }
        public virtual ICollection<Channel> AreAdministratorIn { get; set; }
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } 
        public string Ip { get; set; }
        public string ConnectionId { get; set; }
        public string EmailAddress { get; set; }

        public User()
        {
            Questions = new List<Question>();
        }

        public Question RequestHelp(string question = null)
        {
            if (Channel != null)
            {
                bool help = Channel.RequestHelp(this);
                if (!help) return null;
                if(AreUserQuestioning(Channel))
                    return null;
                Question q = new Question(Channel, question, this);
                //db.Questions.Add(q);
                //Questions.Add(q);
                //db.SaveChanges();
                return q;
            }
            return null;
        }

        public Question GetQuestion(Channel c)
        {
            return Questions.SingleOrDefault(q => q.Channel.Equals(c));
        }

        private void AskQuestion(Channel c, string question)
        {
            if (AreUserQuestioning(c)) 
                return;
            Question q = new Question(c, question, this);
            using (var db = new HelpContext())
            {
                db.Questions.Add(q);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the question for the selected channel
        /// </summary>
        /// <param name="c">The channel Guid to change question in</param>
        /// <param name="question">The question to change to</param>
        /// <returns>true if the question was changed. false if the question was added</returns>
        public bool UpdateQuestion(Channel c, string question)
        {
            /*if (AreUserQuestioning(c))
            {
                return false;
            }*/
            Question q = Questions.SingleOrDefault(qu => qu.Channel.Equals(Channel));
            if (q != null)
            {
                q.Text = question;
                return true;
            }
            return false;
        }

        public void RemoveQuestion(Question q)
        {
            //using (var db = new HelpContext())
            //{
                //Guid channel = db.Channels.Find(ChannelId).Id;
                //var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
                //qu.Remove(channel);
                //Questions = Serialiser.SerialiseDictionary(qu);
                //var question = db.Questions.SingleOrDefault(q => q.Channel.Equals(Channel) && q.User.Equals(this));
                Questions.Remove(q);
                /*var entry = db.Entry(question);
                if (entry.State == EntityState.Detached)
                {
                    db.Questions.Attach(question);
                    db.Questions.Remove(question);
                    db.SaveChanges();
                }*/
            //}
        }

        /*public void RemoveQuestion(Channel c)
        {

            //var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
            //qu.Remove(c);
            //Questions = Serialiser.SerialiseDictionary(qu);
            using (var db = new HelpContext())
            {
                var question = Questions.SingleOrDefault(q => q.Channel.Equals(c));
                db.Questions.Remove(question);
                db.SaveChanges();
            }
        }*/

        public bool AreUserQuestioning(Channel c)
        {
            if (c == null) return false;
            using (var db = new HelpContext())
            {
                //var question = db.Questions.SingleOrDefault(q => q.Channel.Equals(c) && q.User.Equals(this));
                var question = Questions.SingleOrDefault(q => q.Channel.Equals(c));
                return question != null;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }
    }
}