using System;

namespace WhoNeedsHelp.Models
{
    public class QuestionComment
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        public QuestionComment()
        {
        }

        public QuestionComment(User user, string text)
        {
            User = user;
            Text = text;
            Time = DateTime.Now;
        }
    }
}