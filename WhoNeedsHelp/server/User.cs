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
        /// <summary>
        /// Key is the channel Guid
        /// Value is the question text
        /// </summary>
        private readonly Dictionary<Guid, Question> _questions = new Dictionary<Guid, Question>();
        public string Ip { get; set; }
        public string ConnectionId { get; set; }

        public User()
        {
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
                    if (!String.IsNullOrWhiteSpace(question))
                    {
                        AskQuestion(channel.Id, question);
                    }

                    return true;
                }
            }
            return false;
        }

        public Question GetQuestion(Guid c)
        {
            return _questions.ContainsKey(c) ? _questions[c] : null;
        }

        private void AskQuestion(Guid c, string question)
        {
            if (_questions.ContainsKey(c)) return;
            Question q = new Question(c, question, Id);
            _questions.Add(c, q);
        }

        /// <summary>
        /// Updates the question for the selected channel
        /// </summary>
        /// <param name="c">The channel Guid to change question in</param>
        /// <param name="question">The question to change to</param>
        /// <returns>true if the question was changed. false if the question was added</returns>
        public bool UpdateQuestion(Guid c, string question)
        {
            if (!_questions.ContainsKey(c))
            {
                return false;
            }
            Question q = _questions[c];
            q.Text = question;
            _questions[c] = q;
            return true;
        }

        public void RemoveQuestion()
        {
            using (var db = new HelpContext())
            {
                Guid channel = db.Channels.Find(ChannelId).Id;
                _questions.Remove(channel);
            }
        }

        public void RemoveQuestion(Guid c)
        {
            _questions.Remove(c);
        }

        public bool AreUserQuestioning(Guid c)
        {
            return _questions.ContainsKey(c);
        }
    }
}