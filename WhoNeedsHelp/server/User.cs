using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data;

namespace WhoNeedsHelp.server
{
    public class User
    {
        protected bool Equals(User other)
        {
            return Id == other.Id && string.Equals(UserName, other.UserName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id*397) ^ (UserName != null ? UserName.GetHashCode() : 0);
            }
        }

        [Key]
        public int Id { get; set; }

        [UniqueKey]
        public string UserName { get; set; }

        //public ICollection<Connection> Connections { get; set; } 
        public string Name { get; set; }

        public int ChannelId { get; set; }

        [ForeignKey("ChannelId")]
        public Channel Channel { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        [InverseProperty("Users")]
        public virtual ICollection<Channel> ChannelsIn { get; set; }

        [InverseProperty("UsersRequestingHelp")]
        public virtual ICollection<Channel> ChannelsRequestingHelpIn { get; set; }

        [InverseProperty("Administrators")]
        public virtual ICollection<Channel> AreAdministratorIn { get; set; }

        //public string Questions { get; set; }
        public string Ip { get; set; }
        public string ConnectionId { get; set; }

        public User()
        {
            Questions = new List<Question>();
        }

        public bool RequestHelp(string question = null)
        {
            using (var db = new HelpContext())
            {
                if (Channel != null)
                {
                    bool help = Channel.RequestHelp(this);
                    if (!help) return false;
                    if(AreUserQuestioning(Channel))
                        return false;
                    Question q = new Question(Channel, question, this);
                    db.Questions.Add(q);
                    Questions.Add(q);
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
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
            if (AreUserQuestioning(c))
            {
                return false;
            }
            using (var db = new HelpContext())
            {
                Question q = Questions.SingleOrDefault(qu => qu.Channel.Equals(Channel));
                if (q != null)
                {
                    q.Text = question;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public void RemoveQuestion()
        {
            using (var db = new HelpContext())
            {
                //Guid channel = db.Channels.Find(ChannelId).Id;
                //var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
                //qu.Remove(channel);
                //Questions = Serialiser.SerialiseDictionary(qu);
                //var question = db.Questions.SingleOrDefault(q => q.Channel.Equals(Channel) && q.User.Equals(this));
                var question = Questions.SingleOrDefault(q => q.Channel.Equals(Channel));
                db.Questions.Remove(question);
                db.SaveChanges();
            }
        }

        public void RemoveQuestion(Channel c)
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
        }

        public bool AreUserQuestioning(Channel c)
        {
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