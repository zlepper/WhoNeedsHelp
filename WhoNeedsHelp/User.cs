using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp
{
    public class User
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Channel CurrentChannel { get; set; }
        private Dictionary<Channel, string> Questions { get; set; } 

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
            return Questions[c];
        }

        public bool AskQuestion(Channel c, string question)
        {
            if (Questions.ContainsKey(c)) return false;
            Questions.Add(c, question);
            return true;
        }

        public bool UpdateQuestion(Channel c, string question)
        {
            if (!Questions.ContainsKey(c)) return false;
            Questions[c] = question;
            return true;
        }
    }
}