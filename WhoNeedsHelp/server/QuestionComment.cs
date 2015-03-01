using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp.server
{
    public class QuestionComment
    {
        public Guid id { get; set; }
        public Guid User;
        public string Text;
        public DateTime Time;

        public QuestionComment()
        {
        }

        public QuestionComment(Guid user, string text)
        {
            User = user;
            Text = text;
            Time = DateTime.Now;
            using (var db = new HelpContext())
            {
                id = db.GenerateNewGuid(HelpContext.Modes.QuestionComment);
            }
        }
    }
}