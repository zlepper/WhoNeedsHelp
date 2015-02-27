﻿using System;
using System.Data.Entity;
using System.Linq;
using WhoNeedsHelp.server;

namespace WhoNeedsHelp
{
    public class HelpContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        //public DbSet<Connection> Connections { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionComment> QuestionComments { get; set; }

        public Guid GenerateNewGuid(Modes m)
        {
            Guid g;
            switch (m)
            {
                case Modes.User:
                    g = Guid.NewGuid();
                    var u = Users.SingleOrDefault(user => user.Id.Equals(g));
                    while (u != null)
                    {
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
                    var cm = ChatMessages.SingleOrDefault(chatMessage => chatMessage.MessageId.Equals(g));
                    while (cm != null)
                    {
                        g = Guid.NewGuid();
                        cm = ChatMessages.SingleOrDefault(chatMessage => chatMessage.MessageId.Equals(g));
                    }
                    return g;
                case Modes.Question:
                    g = new Guid();
                    var q = Questions.SingleOrDefault(question => question.id.Equals(g));
                    while (q != null)
                    {
                        g = new Guid();
                        q = Questions.SingleOrDefault(question => question.id.Equals(g));
                    }
                    return g;
                case Modes.QuestionComment:
                    g = new Guid();
                    var qc = QuestionComments.SingleOrDefault(questionComment => questionComment.id.Equals(g));
                    while (qc != null)
                    {
                        g = new Guid();
                        qc = QuestionComments.SingleOrDefault(questionComment => questionComment.id.Equals(g));
                    }
                    return g;
                default:
                    throw new ArgumentOutOfRangeException("m");
            }
        }

        public enum Modes
        {
            User,
            Channel,
            ChatMessage,
            Question,
            QuestionComment
        }
    }
}