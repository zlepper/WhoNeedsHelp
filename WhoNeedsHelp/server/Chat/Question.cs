using System;
using WhoNeedsHelp.Simples;

namespace WhoNeedsHelp.Server.Chat
{
    public class Question
    {
        public int Id { get; set; }

        public int ChannelId { get; set; }
        public virtual Channel Channel { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string Text { get; set; }
        //public List<Guid> Comments = new List<Guid>(0);
        public string Comments { get; set; }
        public DateTime AskedTime { get; set; }

        public Question() { }

        public Question(Channel channel, string text, User user)
        {
            Channel = channel;
            Text = text;
            User = user;
            AskedTime = DateTime.Now;
            Comments = "";
        }

        public SimpleQuestion ToSimpleQuestion()
        {
            return new SimpleQuestion(Id, Text, User.ToSimpleUser());
        }


        /*public void AddComment(Guid u, string text)
        {
            
        } */
    }
}