using System;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using MySql.Data.Entity;
using WhoNeedsHelp.server;
using MySql.Data;

namespace WhoNeedsHelp
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class HelpContext : DbContext
    {
        private static int nextUserId = 0,
            nextChannelId = 0,
            nextChatMessageId = 0,
            nextQuestionId = 0,
            nextQuestionCommentId = 0;



        public DbSet<User> Users { get; set; }
        //public DbSet<Connection> Connections { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionComment> QuestionComments { get; set; }

        public HelpContext() : base()
        {
            Database.SetInitializer(new MySqlInitializer());
        }

        /*public Guid GenerateNewGuid(Modes m)
        {
            int g;
            switch (m)
            {
                case Modes.User:
                    g = nextUserId;
                    var u = Users.SingleOrDefault(user => user.Id.Equals(g));
                    while (u != null)
                    {
                        u = Users.
                        g = Guid.NewGuid();
                        u = Users.SingleOrDefault(user => user.Id.Equals(g));
                    }
                    return g;
                case Modes.Channel:
                    g = Guid.NewGuid();
                    var c = Channels.SingleOrDefault(channel => channel.Id.Equals(g));
                    while (c != null)
                    {
                        g = Guid.NewGuid();
                        c = Channels.SingleOrDefault(channel => channel.Id.Equals(g));
                    }
                    return g;
                case Modes.ChatMessage:
                    g = Guid.NewGuid();
                    var cm = ChatMessages.SingleOrDefault(chatMessage => chatMessage.Id.Equals(g));
                    while (cm != null)
                    {
                        g = Guid.NewGuid();
                        cm = ChatMessages.SingleOrDefault(chatMessage => chatMessage.Id.Equals(g));
                    }
                    return g;
                case Modes.Question:
                    g = Guid.NewGuid();
                    var q = Questions.SingleOrDefault(question => question.Id.Equals(g));
                    while (q != null)
                    {
                        g = Guid.NewGuid();
                        q = Questions.SingleOrDefault(question => question.Id.Equals(g));
                    }
                    return g;
                case Modes.QuestionComment:
                    g = Guid.NewGuid();
                    var qc = QuestionComments.SingleOrDefault(questionComment => questionComment.Id.Equals(g));
                    while (qc != null)
                    {
                        g = Guid.NewGuid();
                        qc = QuestionComments.SingleOrDefault(questionComment => questionComment.Id.Equals(g));
                    }
                    return g;
                default:
                    throw new ArgumentOutOfRangeException("m");
            }
        }*/

        /*public enum Modes
        {
            User,
            Channel,
            ChatMessage,
            Question,
            QuestionComment
        }*/
    }
}