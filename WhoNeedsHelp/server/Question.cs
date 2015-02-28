using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoNeedsHelp.server
{
    public class Question
    {
        public Guid Id { get; set; }
        public Guid Channel { get; set; }
        public Guid User { get; set; }
        public string Text { get; set; }
        //public List<Guid> Comments = new List<Guid>(0);
        public string Comments { get; set; }
        public DateTime AskedTime { get; set; }

        public Question() { }

        public Question(Guid channel, string text, Guid user)
        {
            Channel = channel;
            Text = text;
            User = user;
            using (var db = new HelpContext())
            {
                Id = db.GenerateNewGuid(HelpContext.Modes.Question);
            }
            AskedTime = DateTime.Now;
            Comments = "";
        }



        /*public void AddComment(Guid u, string text)
        {
            
        } */
    }
}