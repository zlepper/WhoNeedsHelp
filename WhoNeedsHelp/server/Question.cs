using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp.server
{
    public class Question
    {
        public Channel Channel;
        public User User;
        public string Text;
        public List<QuestionComment> Comments = new List<QuestionComment>(0);

        public Question(Channel channel, string text, User user)
        {
            Channel = channel;
            Text = text;
            User = user;
        }

        public void AddComment(User u, string text)
        {
            Comments.Add(new QuestionComment(u, text));
        } 
    }
}