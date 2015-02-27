using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.Expressions;

namespace WhoNeedsHelp
{
    public class User
    {
        
        public string UserName { get; set; }

        public ICollection<Connection> Connections { get; set; } 
        public string Name { get; set; }
        public string CurrentChannelId { get; set; }
        private Dictionary<Channel, string> Questions { get; set; }
        public string ip { get; set; }
        [Key]
        public string ConnectionId { get; set; }

        public User()
        {
            Questions = new Dictionary<Channel, string>();
        }

        public bool RequestHelp(string question = null)
        {
            using (var db = new HelpContext())
            {
                Channel channel = db.Channels.Find(CurrentChannelId);
                if (channel != null)
                {
                    bool help = channel.RequestHelp(this);
                    if (!help) return false;
                    if (!String.IsNullOrWhiteSpace(question))
                    {
                        AskQuestion(channel, question);
                    }

                    return true;
                }
            }
            
        }

        public string GetQuestion(Channel c)
        {
            return Questions.ContainsKey(c) ? Questions[c] : "";
        }

        private void AskQuestion(Channel c, string question)
        {
            if (Questions.ContainsKey(c)) return;
            Questions.Add(c, String.IsNullOrWhiteSpace(question) ? "" : question);
        }

        /// <summary>
        /// Updates the question for the selected channel
        /// </summary>
        /// <param name="c">The channel to change question in</param>
        /// <param name="question">The question to change to</param>
        /// <returns>true if the question was changed. false if the question was added</returns>
        public bool UpdateQuestion(Channel c, string question)
        {
            if (!Questions.ContainsKey(c))
            {
                Questions.Add(c, String.IsNullOrWhiteSpace(question) ? "" : question);
                return false;
            }
            Questions[c] = question;
            return true;
        }

        public void RemoveQuestion()
        {
            Questions.Remove(CurrentChannel);
        }

        public void RemoveQuestion(Channel c)
        {
            Questions.Remove(c);
        }

        public bool AreUserQuestioning(Channel c)
        {
            return Questions.ContainsKey(c);
        }
    }
}