using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp.server
{
    public class QuestionComment
    {
        public User User;
        public string Text;
        public DateTime Time;

        public QuestionComment(User user, string text)
        {
            User = user;
            Text = text;
            Time = DateTime.Now;
        }
    }
}