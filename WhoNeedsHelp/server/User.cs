using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhoNeedsHelp.server
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string UserName { get; set; }

        //public ICollection<Connection> Connections { get; set; } 
        public string Name { get; set; }
        public Guid ChannelId { get; set; }
        public string Questions { get; set; }
        public string Ip { get; set; }
        public string ConnectionId { get; set; }

        public User()
        {
            Questions = "";
            using (var db = new HelpContext())
            {
                Id = db.GenerateNewGuid(HelpContext.Modes.User);
            }
        }

        public bool RequestHelp(string question = null)
        {
            using (var db = new HelpContext())
            {
                Channel channel = db.Channels.Find(ChannelId);
                if (channel != null)
                {
                    bool help = channel.RequestHelp(Id);
                    if (!help) return false;
                    //AskQuestion(channel.Id, question);
                    if (Questions.Contains(channel.Id + ":"))
                        return false;
                    Question q = new Question(channel.Id, question, Id);
                    db.Questions.Add(q);
                    var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
                    qu.Add(channel.Id, q.Id);
                    Questions = Serialiser.SerialiseDictionary(qu);
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public Guid GetQuestion(Guid c)
        {
            if (Questions.Contains(c + ":"))
            {
                var g = Serialiser.DesiraliseGuidStringDictionary(Questions);
                return g[c];
            }
            return Guid.Empty;
        }

        private void AskQuestion(Guid c, string question)
        {
            if (Questions.Contains(c + ":")) 
                return;
            Question q = new Question(c, question, Id);
            using (var db = new HelpContext())
            {
                db.Questions.Add(q);
                var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
                qu.Add(c, q.Id);
                Questions = Serialiser.SerialiseDictionary(qu);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the question for the selected channel
        /// </summary>
        /// <param name="c">The channel Guid to change question in</param>
        /// <param name="question">The question to change to</param>
        /// <returns>true if the question was changed. false if the question was added</returns>
        public bool UpdateQuestion(Guid c, string question)
        {
            if (!Questions.Contains(c + ":"))
            {
                return false;
            }
            var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
            Guid qGuid = qu[c];
            using (var db = new HelpContext())
            {
                Question q = db.Questions.Find(qGuid);
                q.Text = question;
                db.SaveChanges();
                return true;
            }
        }

        public void RemoveQuestion()
        {
            using (var db = new HelpContext())
            {
                Guid channel = db.Channels.Find(ChannelId).Id;
                var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
                qu.Remove(channel);
                Questions = Serialiser.SerialiseDictionary(qu);
            }
        }

        public void RemoveQuestion(Guid c)
        {

            var qu = Serialiser.DesiraliseGuidStringDictionary(Questions);
            qu.Remove(c);
            Questions = Serialiser.SerialiseDictionary(qu);
        }

        public bool AreUserQuestioning(Guid c)
        {
            return Questions.Contains(c + ":");
        }
    }
}