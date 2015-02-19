using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.Expressions;

namespace WhoNeedsHelp
{
    public class User
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Channel CurrentChannel { get; set; }
        private Dictionary<Channel, string> Questions { get; set; }

        public User()
        {
            Questions = new Dictionary<Channel, string>();
        }

        public bool RequestHelp(string question = null)
        {
            bool help = CurrentChannel.RequestHelp(this);
            if (!help) return false;
            if (!String.IsNullOrWhiteSpace(question))
            {
                AskQuestion(CurrentChannel, question);
            }
            return true;
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
    }
}